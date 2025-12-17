using System;
using System.Globalization;

namespace zPdfGenerator.Globalization
{
    internal sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo? _previousCulture;
        private readonly CultureInfo? _previousUICulture;
        private readonly bool _changed;

        private CultureScope(CultureInfo previousCulture, CultureInfo previousUICulture, CultureInfo newCulture)
        {
            _previousCulture = previousCulture;
            _previousUICulture = previousUICulture;
            _changed = true;

            CultureInfo.CurrentCulture = newCulture;
            CultureInfo.CurrentUICulture = newCulture;
        }

        private CultureScope() => _changed = false;

        internal static CultureScope Use(CultureInfo? culture)
        {
            if (culture is null)
                return new CultureScope(); // no-op

            return new CultureScope(
                CultureInfo.CurrentCulture,
                CultureInfo.CurrentUICulture,
                culture);
        }

        public void Dispose()
        {
            if (!_changed) return;

            CultureInfo.CurrentCulture = _previousCulture!;
            CultureInfo.CurrentUICulture = _previousUICulture!;
        }
    }
}
