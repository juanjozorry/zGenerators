using System.Globalization;
using Xunit;
using zPdfGenerator.Globalization;

namespace zPdfGenerator.Tests.Other
{
    public sealed class CultureScopeTests
    {
        [Fact]
        public void Use_SetsCurrentCulture_AndRestoresOnDispose()
        {
            var original = CultureInfo.CurrentCulture;
            var originalUi = CultureInfo.CurrentUICulture;

            var target = CultureInfo.GetCultureInfo("es-ES");

            using (CultureScope.Use(target))
            {
                Assert.Equal("es-ES", CultureInfo.CurrentCulture.Name);
                Assert.Equal("es-ES", CultureInfo.CurrentUICulture.Name);
            }

            Assert.Equal(original.Name, CultureInfo.CurrentCulture.Name);
            Assert.Equal(originalUi.Name, CultureInfo.CurrentUICulture.Name);
        }

        [Fact]
        public void Use_AllowsNestedScopes_AndRestoresCorrectly()
        {
            var original = CultureInfo.CurrentCulture;
            var originalUi = CultureInfo.CurrentUICulture;

            var c1 = CultureInfo.GetCultureInfo("es-ES");
            var c2 = CultureInfo.GetCultureInfo("en-US");

            using (CultureScope.Use(c1))
            {
                Assert.Equal("es-ES", CultureInfo.CurrentCulture.Name);

                using (CultureScope.Use(c2))
                {
                    Assert.Equal("en-US", CultureInfo.CurrentCulture.Name);
                    Assert.Equal("en-US", CultureInfo.CurrentUICulture.Name);
                }

                Assert.Equal("es-ES", CultureInfo.CurrentCulture.Name);
                Assert.Equal("es-ES", CultureInfo.CurrentUICulture.Name);
            }

            Assert.Equal(original.Name, CultureInfo.CurrentCulture.Name);
            Assert.Equal(originalUi.Name, CultureInfo.CurrentUICulture.Name);
        }

        [Fact]
        public void Use_RestoresCulture_EvenIfExceptionIsThrown()
        {
            var original = CultureInfo.CurrentCulture;
            var originalUi = CultureInfo.CurrentUICulture;

            var target = CultureInfo.GetCultureInfo("fr-FR");

            try
            {
                using (CultureScope.Use(target))
                {
                    Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
                    throw new InvalidOperationException("boom");
                }
            }
            catch (InvalidOperationException)
            {
            }

            Assert.Equal(original.Name, CultureInfo.CurrentCulture.Name);
            Assert.Equal(originalUi.Name, CultureInfo.CurrentUICulture.Name);
        }

        [Fact]
        public void Use_WithNullCulture_LeavesCultureUnchanged()
        {
            var original = CultureInfo.CurrentCulture;
            var originalUi = CultureInfo.CurrentUICulture;

            using (CultureScope.Use(null))
            {
                Assert.Equal(original.Name, CultureInfo.CurrentCulture.Name);
                Assert.Equal(originalUi.Name, CultureInfo.CurrentUICulture.Name);
            }

            Assert.Equal(original.Name, CultureInfo.CurrentCulture.Name);
            Assert.Equal(originalUi.Name, CultureInfo.CurrentUICulture.Name);
        }

        [Fact]
        public async Task Use_DoesNotAffectOtherThreads()
        {
            var baseline = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentCulture = baseline;
            CultureInfo.CurrentUICulture = baseline;

            var otherThreadCultureBefore = "";
            var otherThreadCultureDuring = "";
            var otherThreadCultureAfter = "";

            var t = Task.Run(async () =>
            {
                otherThreadCultureBefore = CultureInfo.CurrentCulture.Name;

                await Task.Delay(50);
                otherThreadCultureDuring = CultureInfo.CurrentCulture.Name;

                await Task.Delay(50);
                otherThreadCultureAfter = CultureInfo.CurrentCulture.Name;
            });

            var target = CultureInfo.GetCultureInfo("es-ES");

            using (CultureScope.Use(target))
            {
                Assert.Equal("es-ES", CultureInfo.CurrentCulture.Name);
                await Task.Delay(75);
            }

            await t;

            Assert.Equal(otherThreadCultureBefore, otherThreadCultureDuring);
            Assert.Equal(otherThreadCultureDuring, otherThreadCultureAfter);

            Assert.Equal("en-US", CultureInfo.CurrentCulture.Name);
            Assert.Equal("en-US", CultureInfo.CurrentUICulture.Name);
        }
    }
}
