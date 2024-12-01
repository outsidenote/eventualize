using Cocona;
using Cocona.Builder;
using EvDb.MinimalStructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class PostgresStressTests : StressBaseTests
{
    #region Ctor

    public PostgresStressTests(ITestOutputHelper output) : base(output, StoreType.Postgres)
    {
    }

    #endregion //  Ctor
}