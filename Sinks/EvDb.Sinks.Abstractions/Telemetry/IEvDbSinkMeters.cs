using System.Diagnostics.Metrics;

namespace EvDb.Sinks;

public interface IEvDbSinkMeters
{
    /// <summary>
    /// The meter name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Events stored into the storage database
    /// </summary>
    Counter<int> Published { get; }
}
