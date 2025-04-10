
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStreamConfig
{
    EvDbPartitionAddress PartitionAddress { get; }

    JsonSerializerOptions? Options { get; }

    TimeProvider TimeProvider { get; }
}
