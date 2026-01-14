using System;
using System.IO;
using System.Text;
using zPdfGenerator.Html;

namespace zPdfGenerator.Tests.Html
{
    public class FilteringResourceRetrieverTests
    {
        [Fact]
        public void GetByteArrayByUrl_ReturnsNull_WhenSchemeNotAllowed()
        {
            var policy = new HtmlResourceAccessPolicy()
                .AllowSchemes("file");

            var retriever = new FilteringResourceRetriever(policy);
            var result = retriever.GetByteArrayByUrl(new Uri("http://example.com/resource"));

            Assert.Null(result);
        }

        [Fact]
        public void GetByteArrayByUrl_ReturnsBytes_ForAllowedFile()
        {
            var content = "hello";
            var tempPath = Path.Combine(Path.GetTempPath(), $"resource_{Guid.NewGuid():N}.txt");
            File.WriteAllText(tempPath, content, Encoding.UTF8);

            var policy = new HtmlResourceAccessPolicy()
                .AllowSchemes("file");

            var retriever = new FilteringResourceRetriever(policy);
            var bytes = retriever.GetByteArrayByUrl(new Uri(tempPath));

            Assert.NotNull(bytes);
            var expected = File.ReadAllBytes(tempPath);
            Assert.Equal(expected, bytes);
        }

        [Fact]
        public void GetInputStreamByUrl_ReturnsNull_WhenHostNotAllowed()
        {
            var policy = new HtmlResourceAccessPolicy()
                .AllowHosts("allowed.example.com");

            var retriever = new FilteringResourceRetriever(policy);
            var stream = retriever.GetInputStreamByUrl(new Uri("http://blocked.example.com/resource"));

            Assert.Null(stream);
        }
    }
}
