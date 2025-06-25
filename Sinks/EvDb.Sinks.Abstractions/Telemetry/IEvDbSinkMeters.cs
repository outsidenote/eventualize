using System.Diagnostics.Metrics;

namespace EvDb.Sinks;

public interface IEvDbSinkMeters
{
    /// <summary>
    /// The meter name
    /// </summary>
    string Name { get; }

    public void IncrementPublish(EvDbSinkTarget target);
}
