using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

public static class EvDbEventFactory 
{
    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ?? throw new NotSupportedException("GetExecutingAssembly");
    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";

    public static IEvDbEvent Create<T>(
        T data,
        string? capturedBy = null,
        JsonSerializerOptions? options = null)
        where T: IEvDbEventPayload
    {
        capturedBy = capturedBy ?? DEFAULT_CAPTURE_BY;
        var json = JsonSerializer.Serialize(data, options);
        var result = new EvDbEvent(data.EventType, DateTime.UtcNow, capturedBy, json);
        return result;
    }

    public static IEvDbEvent Create<T>(
        T data,
        JsonSerializerOptions? options,
        string? capturedBy = null)
        where T: IEvDbEventPayload
    {
        var result = Create(data, capturedBy, options);

        return result;
    }

    //public static IEvDbEvent Create<T>(
    //    T data, string capturedBy, 
    //    JsonTypeInfo<T> jsonType)
    //    where T : IEvDbEventPayload
    //{
    //    var json = JsonSerializer.Serialize(data, jsonType);
    //    var result = new EvDbEvent(data.EventType, DateTime.UtcNow, capturedBy, json);
    //    return result;
    //}
}

