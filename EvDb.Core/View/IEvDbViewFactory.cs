
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbViewFactory
{ 
    string ViewName { get; }

    IEvDbView CreateEmpty(EvDbStreamAddress address, JsonSerializerOptions? options);

    IEvDbView CreateFromSnapshot(EvDbStreamAddress address,
        EvDbStoredSnapshot snapshot,
        JsonSerializerOptions? options);
}
