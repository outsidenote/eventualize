using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

//public interface IEvDbEventFactory
//{
//    IEvDbEvent Create<T>(T data, string capturedBy, Options? options = null)
//        where T : IEvDbEventPayload;
//    IEvDbEvent Create<T>(T data, string capturedBy, JsonTypeInfo<T> jsonType)
//        where T : IEvDbEventPayload;
//}

[Obsolete("deprecated")]
public interface IEvDbEventFactory<T>
{
    public IEvDbEvent Create(DateTime capturedAt, T data, JsonSerializerOptions? options = null);
    public IEvDbEvent Create(T data, JsonSerializerOptions? options = null);
    public IEvDbEvent Create(DateTime capturedAt, T data, JsonTypeInfo<T> jsonType);
    public IEvDbEvent Create(T data, JsonTypeInfo<T> jsonType);
}