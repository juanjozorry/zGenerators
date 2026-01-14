using zPdfGenerator.Html;

namespace zPdfGenerator.Tests.Html
{
    public class HtmlResourceAccessPolicyTests
    {
        [Fact]
        public void AllowSchemes_RejectsNull()
        {
            var policy = new HtmlResourceAccessPolicy();
            Assert.Throws<ArgumentNullException>(() => policy.AllowSchemes(null!));
        }

        [Fact]
        public void AllowSchemes_RejectsEmptyEntries()
        {
            var policy = new HtmlResourceAccessPolicy();
            Assert.Throws<ArgumentException>(() => policy.AllowSchemes(""));
        }

        [Fact]
        public void AllowHosts_RejectsNull()
        {
            var policy = new HtmlResourceAccessPolicy();
            Assert.Throws<ArgumentNullException>(() => policy.AllowHosts(null!));
        }

        [Fact]
        public void AllowHosts_RejectsEmptyEntries()
        {
            var policy = new HtmlResourceAccessPolicy();
            Assert.Throws<ArgumentException>(() => policy.AllowHosts(" "));
        }

        [Fact]
        public void AllowSchemes_IsCaseInsensitive_AndDeduplicates()
        {
            var policy = new HtmlResourceAccessPolicy();

            policy.AllowSchemes("HTTP", "http");

            Assert.Single(policy.AllowedSchemes);
            Assert.Contains("http", policy.AllowedSchemes);
        }

        [Fact]
        public void AllowHosts_IsCaseInsensitive_AndDeduplicates()
        {
            var policy = new HtmlResourceAccessPolicy();

            policy.AllowHosts("cdn.example.com", "CDN.EXAMPLE.COM");

            Assert.Single(policy.AllowedHosts);
            Assert.Contains("cdn.example.com", policy.AllowedHosts);
        }
    }
}
