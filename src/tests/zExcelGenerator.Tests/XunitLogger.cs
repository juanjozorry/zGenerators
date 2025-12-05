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

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true; 

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter != null)
            {
                var message = formatter(state, exception);
                _output.WriteLine($"[{logLevel}] {message}");

                if (exception != null)
                    _output.WriteLine(exception.ToString());
            }
        }

        private class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();
            public void Dispose() { }
        }
    }
}
