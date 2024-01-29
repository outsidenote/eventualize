using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStreamStoreData
{
    JsonSerializerOptions? Options { get; }

    IEnumerable<IEvDbView> Views { get; }

    IEnumerable<IEvDbEvent> Events { get; }
}
