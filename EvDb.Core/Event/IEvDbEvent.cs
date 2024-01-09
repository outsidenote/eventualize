using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

// TODO: Json data -> JsonElement if needed at all

public interface IEvDbEventMeta
{
    string EventType { get; }
    DateTime CapturedAt { get; }
    string CapturedBy { get; }
}

public interface IEvDbEvent: IEvDbEventMeta
{
    T GetData<T>(JsonSerializerOptions? options = null);
    //T GetData<T>(JsonTypeInfo<T> context);
}
