namespace EvDb.Core.Tests;

using EvDb.UnitTests;

using Scenes;
using System.Text.Json;

public class SerializationTests
{
    [Fact(Skip = "Polymorphism (IEvDbEventPayload)")]
    public void CourseCreatedEvent_Serialization_Succeed()
    {
        IEvDbEventPayload payload = new CourseCreatedEvent(1234, "Anatomy", 40);
        var jdef = StudentFlowEventTypesContext.Default;
        JsonSerializerOptions options = null;
        var json = JsonSerializer.Serialize(payload, options);
        var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        Assert.Equal(payload, deserialized);
    }

    [Fact]
    public void CourseCreatedEvent_Serialization_T_Succeed()
    {
        var payload = new CourseCreatedEvent(1234, "Anatomy", 40);
        var jdef = StudentFlowEventTypesContext.Default;
        JsonSerializerOptions options = null;
        var json = JsonSerializer.Serialize(payload, options);
        var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        Assert.Equal(payload, deserialized);
    }

    [Fact(Skip = "Polymorphism (IEvDbEventPayload)")]
    public void CourseCreatedEvent_Serialization_Converter_Succeed()
    {
        IEvDbEventPayload payload = new CourseCreatedEvent(1234, "Anatomy", 40);
        var jdef = StudentFlowEventTypesContext.Default;
        JsonSerializerOptions options = new();
        options.Converters.Add(jdef.CourseCreatedEvent.Converter);
        options.Converters.Add(jdef.ScheduleTestEvent.Converter);
        var json = JsonSerializer.Serialize(payload, options);
        var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        Assert.Equal(payload, deserialized);
    }

    [Fact(Skip = "Polymorphism (IEvDbEventPayload)")]
    public void CourseCreatedEvent_Serialization_Resolver_Succeed()
    {
        IEvDbEventPayload payload = new CourseCreatedEvent(1234, "Anatomy", 40);

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = StudentFlowEventTypesContext.Default
        };

        var json = JsonSerializer.Serialize(payload, options);
        var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        Assert.Equal(payload, deserialized);
    }

    [Fact]
    public void CourseCreatedEvent_Serialization_Resolver1_Succeed()
    {
        IEvDbEventPayload payload = new CourseCreatedEvent(1234, "Anatomy", 40);

        var options = StudentFlowEventTypesContext.Default.CourseCreatedEvent;

        var json = JsonSerializer.Serialize(payload, options);
        var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        Assert.Equal(payload, deserialized);
    }

    [Fact]
    public void CourseCreatedEvent_Serialization_Null_Succeed()
    {
        var payload = new Test1("Anatomy", 40);

        var json = JsonSerializer.Serialize(payload);
        var deserialized = JsonSerializer.Deserialize<Test1>(json);

        Assert.Equal(payload, deserialized);
    }
}

public record Test1(string Name, int I);