using iText.StyledXmlParser.Resolver.Resource;
using System;
using System.Collections.Generic;
using System.IO;

namespace zPdfGenerator.Html
{
    /// <summary>
    /// Defines allowlist rules for external resource access during HTML to PDF conversion.
    /// </summary>
    public sealed class HtmlResourceAccessPolicy
    {
        private readonly HashSet<string> _allowedSchemes = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _allowedHosts = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the allowed URI schemes. If empty, all schemes are allowed.
        /// </summary>
        public IReadOnlyCollection<string> AllowedSchemes => _allowedSchemes;

        /// <summary>
        /// Gets the allowed hosts. If empty, all hosts are allowed.
        /// </summary>
        public IReadOnlyCollection<string> AllowedHosts => _allowedHosts;

        internal bool HasRestrictions => _allowedSchemes.Count > 0 || _allowedHosts.Count > 0;

        /// <summary>
        /// Adds allowed URI schemes to the policy.
        /// </summary>
        /// <param name="schemes">The schemes to allow (e.g. "file", "http", "https", "data").</param>
        /// <returns>The current policy instance.</returns>
        public HtmlResourceAccessPolicy AllowSchemes(params string[] schemes)
        {
            if (schemes is null) throw new ArgumentNullException(nameof(schemes));

            foreach (var scheme in schemes)
            {
                if (string.IsNullOrWhiteSpace(scheme))
                    throw new ArgumentException("Scheme cannot be null or empty.", nameof(schemes));

                _allowedSchemes.Add(scheme.Trim());
            }

            return this;
        }

        /// <summary>
        /// Adds allowed hosts to the policy.
        /// </summary>
        /// <param name="hosts">The hosts to allow (e.g. "cdn.example.com").</param>
        /// <returns>The current policy instance.</returns>
        public HtmlResourceAccessPolicy AllowHosts(params string[] hosts)
        {
            if (hosts is null) throw new ArgumentNullException(nameof(hosts));

            foreach (var host in hosts)
            {
                if (string.IsNullOrWhiteSpace(host))
                    throw new ArgumentException("Host cannot be null or empty.", nameof(hosts));

                _allowedHosts.Add(host.Trim());
            }

            return this;
        }

        internal bool AllowsScheme(string scheme)
        {
            if (_allowedSchemes.Count == 0) return true;
            return _allowedSchemes.Contains(scheme);
        }

        internal bool AllowsHost(string host)
        {
            if (_allowedHosts.Count == 0) return true;
            if (string.IsNullOrWhiteSpace(host)) return true;
            return _allowedHosts.Contains(host);
        }
    }

    internal sealed class FilteringResourceRetriever : IResourceRetriever
    {
        private readonly HtmlResourceAccessPolicy _policy;
        private readonly DefaultResourceRetriever _inner = new();

        public FilteringResourceRetriever(HtmlResourceAccessPolicy policy)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public Stream? GetInputStreamByUrl(Uri url)
        {
            if (!IsAllowed(url)) return null;
            return _inner.GetInputStreamByUrl(url);
        }

        public byte[]? GetByteArrayByUrl(Uri url)
        {
            if (!IsAllowed(url)) return null;
            return _inner.GetByteArrayByUrl(url);
        }

        private bool IsAllowed(Uri? url)
        {
            if (url is null) return false;
            if (!_policy.AllowsScheme(url.Scheme)) return false;
            if (!_policy.AllowsHost(url.Host)) return false;
            return true;
        }
    }
}
