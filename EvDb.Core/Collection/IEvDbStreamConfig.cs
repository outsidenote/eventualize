
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStreamConfig
{
    EvDbPartitionAddress Partition { get; }
    string Kind { get; }

    int MinEventsBetweenSnapshots { get; }

    JsonSerializerOptions? JsonSerializerOptions { get; }
}
