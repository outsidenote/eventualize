namespace EvDb.Core;

[Flags]
public enum EvDbMeters
{
    None,
    SystemCounters = 1,
    SystemDuration = SystemCounters * 2,
    All = SystemCounters | SystemDuration,
}

