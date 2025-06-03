using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class XUnitLogger : ILogger
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _categoryName;

    public XUnitLogger(ITestOutputHelper testOutputHelper, string categoryName)
    {
        _testOutputHelper = testOutputHelper;
        _categoryName = categoryName;
    }


    bool ILogger.IsEnabled(LogLevel logLevel) => true;

    void ILogger.Log<TState>(LogLevel logLevel,
                             EventId eventId,
                             TState state,
                             Exception? exception,
                             Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _testOutputHelper.WriteLine($"[{logLevel}] {_categoryName}: {message}");

        if (exception != null)
        {
            _testOutputHelper.WriteLine($"Exception: {exception}");
        }
    }

    IDisposable? ILogger.BeginScope<TState>(TState state) => null;
}