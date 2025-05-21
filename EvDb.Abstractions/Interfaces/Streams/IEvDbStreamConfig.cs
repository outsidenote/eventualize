
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStreamConfig
{
    EvDbRootAddressName RootAddress { get; }

    JsonSerializerOptions? Options { get; }

    TimeProvider TimeProvider { get; }
}
