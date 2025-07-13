---
layout: default
title: Tests
nav_order: 4
parent: Quick Start
grand_parent: Languages
has_children: true
---

# Tests

## Add Integration Tests project

- Add New xUnit Test Project (_named: EvDbQuickStart.Funds.IntegrationTests_)

### Add Reference

- Add Project reference to `EvDbQuickStart.Funds.Repositories`

Add NuGets

```bash
dotnet add Microsoft.Extensions.Configuration.Json
dotnet add Microsoft.Extensions.Logging
dotnet add Microsoft.Extensions.Logging.Configuration
dotnet add package FakeItEasy
```

### Add Custom Test Logger class

```cs
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;


namespace EvDbQuickStart.Funds.IntegrationTests;

public class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public XUnitLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }

    public ILogger CreateLogger(string categoryName)
        => new XUnitLogger(_output, categoryName);

    public void Dispose() { }

    private class XUnitLogger : ILogger
    {
        private readonly ITestOutputHelper _output;
        private readonly string _categoryName;

        public XUnitLogger(ITestOutputHelper output, string categoryName)
        {
            _output = output;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            _output.WriteLine($"{logLevel} [{_categoryName}] {message}");

            if (exception is not null)
                _output.WriteLine(exception.ToString());
        }
    }
}
```

---

### Choose the scenario

- [Test Stream + View with MongoDB](test-view-mongodb)
- [Test Stream + View with Postgres](test-view-postgres)
- [Test Stream + View with Sql-Server](test-view-sql-server)
