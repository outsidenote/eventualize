using System.Collections.Immutable;
using System.Diagnostics.Metrics;

namespace EvDb.Core.Adapters;

internal interface IEvDbStoreMeters
{
    /// <summary>
    /// Events stored into the storage database
    /// </summary>
    void AddEvents(IImmutableList<EvDbEvent> events);
    /// <summary>
    /// Events stored into the storage database
    /// </summary>
    void AddMessages(IImmutableList<EvDbMessage> events);
}
