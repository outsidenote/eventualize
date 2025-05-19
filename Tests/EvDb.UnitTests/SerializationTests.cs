namespace EvDb.Core.Tests;

using Scenes;
using System.Text.Json;


public class SerializationTests
{
    [Fact(Skip = "Polymorphism (IEvDbPayload)")]
    public void CourseCreatedEvent_Serialization_Succeed()
    {
        IEvDbPayload payload = new CourseCreatedEvent(1234, "Anatomy", 40);
        JsonSerializerOptions? options = null;
        var json = JsonSerializer.Serialize(payload, options);
        var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        Assert.Equal(payload, deserialized);
    }

    [Fact]
    public void CourseCreatedEvent_Serialization_T_Succeed()
    {
        var payload = new CourseCreatedEvent(1234, "Anatomy", 40);
        JsonSerializerOptions? options = null;
        var json = JsonSerializer.Serialize(payload, options);
        var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        Assert.Equal(payload, deserialized);
    }

#pragma warning disable S125 // Sections of code should not be commented out
#pragma warning disable S2699 // Tests should include assertions
    [Fact(Skip = "Polymorphism (IEvDbPayload)")]
    public void CourseCreatedEvent_Serialization_Converter_Succeed()
    {
        throw new NotImplementedException();
        //IEvDbPayload payload = new CourseCreatedEvent(1234, "Anatomy", 40);
        //var jdef = SchoolStreamSerializationContext.Default;
        //JsonSerializerOptions options = new();
        //options.Converters.AppendToOutbox(jdef.CourseCreatedEvent.Converter);
        //options.Converters.AppendToOutbox(jdef.ScheduleTestEvent.Converter);
        //var json = JsonSerializer.Serialize(payload, options);
        //var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        //Assert.Equal(payload, deserialized);
    }
#pragma warning restore S2699 // Tests should include assertions
#pragma warning restore S125 // Sections of code should not be commented out

#pragma warning disable S125 // Sections of code should not be commented out
#pragma warning disable S2699 // Tests should include assertions
    [Fact(Skip = "Polymorphism (IEvDbPayload)")]
    public void CourseCreatedEvent_Serialization_Resolver_Succeed()
    {
        throw new NotImplementedException();
        //IEvDbPayload payload = new CourseCreatedEvent(1234, "Anatomy", 40);

        //var options = new JsonSerializerOptions
        //{
        //    TypeInfoResolver = SchoolStreamSerializationContext.Default
        //};

        //var json = JsonSerializer.Serialize(payload, options);
        //var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        //Assert.Equal(payload, deserialized);
    }
#pragma warning restore S2699 // Tests should include assertions
#pragma warning restore S125 // Sections of code should not be commented out

#pragma warning disable S125 // Sections of code should not be commented out
#pragma warning disable S2699 // Tests should include assertions
    [Fact(Skip = "Record source code serialization not supported yet (readonly property)")]
    public void CourseCreatedEvent_Serialization_Resolver1_Succeed()
    {
        throw new NotImplementedException();
        //IEvDbPayload payload = new CourseCreatedEvent(1234, "Anatomy", 40);

        //var options = SchoolStreamSerializationContext.Default.CourseCreatedEvent;

        //var json = JsonSerializer.Serialize(payload, options);
        //var deserialized = JsonSerializer.Deserialize<CourseCreatedEvent>(json, options);

        //Assert.Equal(payload, deserialized);
    }
#pragma warning restore S2699 // Tests should include assertions
#pragma warning restore S125 // Sections of code should not be commented out

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