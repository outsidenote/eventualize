using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

// TODO: Json data -> JsonElement if needed at all

public interface IEventualizeEvent
{ 
    string EventType { get; }
    DateTime CapturedAt { get; }
    string CapturedBy { get; }

    T GetData<T>(JsonSerializerOptions? options = null);
    T GetData<T>(JsonTypeInfo<T> context);
}
