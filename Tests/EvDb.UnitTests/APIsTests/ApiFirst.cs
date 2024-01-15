using EvDb.Core;
using EvDb.Scenes;
using FakeItEasy;
using System.CodeDom.Compiler;

namespace EvDb.UnitTests;

public class ApiFirst
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();

    [Fact]
    public async Task ApiGeneratedDesign()
    {
        BestStudentFactory factory = new BestStudentFactory(_storageAdapter);
        IBestStudent agg = factory.Create("class a-3");
        var course = new CourseCreatedEvent(123, "algorithm", 50);
        agg.Add(course);
    }

    [Fact]
    public async Task ApiManualDesign()
    {
        TopStudentFactory factory = new TopStudentFactory(_storageAdapter);
        var agg = factory.Create("class a-3");
        var course = new CourseCreatedEvent(123, "algorithm", 50);
        agg.Add(course);
    }
}
