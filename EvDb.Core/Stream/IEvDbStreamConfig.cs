
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStreamConfig
{
    EvDbPartitionAddress PartitionAddress { get; }

    int MinEventsBetweenSnapshots { get; }

    JsonSerializerOptions? Options { get; }
}
