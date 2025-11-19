namespace EvDb.Core;

internal abstract class EvDbTelemetryInternal: EvDbTelemetry
{
    public static IEvDbSysMeters SysMeters { get; } = new EvDbSysMeters();
}

