
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStreamConfig
{
    EvDbStreamTypeName StreamType { get; }

    JsonSerializerOptions? Options { get; }

    TimeProvider TimeProvider { get; }
}
