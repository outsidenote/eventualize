using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

public interface IEventualizeEventFactory<T>
{
    public IEventualizeEvent Create(DateTime capturedAt, T data, JsonSerializerOptions? options = null);
    public IEventualizeEvent Create(T data, JsonSerializerOptions? options = null);
    public IEventualizeEvent Create(DateTime capturedAt, T data, JsonTypeInfo<T> jsonType);
    public IEventualizeEvent Create(T data, JsonTypeInfo<T> jsonType);
}