using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace zExcelGenerator.Tests
{
    public class XunitLogger<T> : ILogger<T>
    {
        private readonly ITestOutputHelper _output;

        public XunitLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true; // siempre activo en tests

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                var message = formatter(state, exception);
                _output.WriteLine($"[{logLevel}] {message}");

                if (exception != null)
                    _output.WriteLine(exception.ToString());
            }
        }
    }

}
