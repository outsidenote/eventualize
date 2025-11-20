// Ignore Spelling: Sys

using System.Diagnostics;

namespace EvDb.Core;

internal static class EvDbTelemetryInternal
{
    public const string TraceName = "EvDb";

    public static ActivitySource Trace { get; } = new ActivitySource(TraceName);

    public static IEvDbSysMeters SysMeters { get; } = new EvDbSysMeters();
}

