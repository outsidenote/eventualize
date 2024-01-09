using EvDb.Core;
using EvDb.Scenes;
using FakeItEasy;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EvDb.UnitTests;

[JsonSerializable(typeof(CourseCreated))]
[JsonSerializable(typeof(ScheduleTest))]
[JsonSerializable(typeof(StudentAppliedToCourse))]
[JsonSerializable(typeof(StudentCourseApplicationDenied))]
[JsonSerializable(typeof(StudentEnlisted))]
[JsonSerializable(typeof(StudentQuitCourse))]
[JsonSerializable(typeof(StudentReceivedGrade))]
[JsonSerializable(typeof(StudentRegisteredToCourse))]
[JsonSerializable(typeof(StudentTestSubmitted))]
public partial class EducationEventTypesContext : JsonSerializerContext
{
}

public class ApiFirst
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();

    [Fact]
    public async Task ApiDesign()
    {
        TopStudentFactory factory = new TopStudentFactory(_storageAdapter);
        var agg = factory.Create("class a-3");
        var course = new CourseCreated(123, "algorithm", 50);
        agg.Add(course);
    }
}

// TODO: [bnaya 2024-01-07] should be a factory
[EvDbAggregateFactory<ICollection<StudentScore>, IEducationEventTypes>]
public partial class TopStudentFactory 
{    
    private readonly ConcurrentDictionary<int, StudentEntity> _students = new ConcurrentDictionary<int, StudentEntity>();

    protected override ICollection<StudentScore> DefaultState { get; } = [];

    public override string Kind { get; } = "top-student";

    protected override JsonSerializerOptions? JsonSerializerOptions { get; } = EducationEventTypesContext.Default.Options;

    public override EvDbPartitionAddress Partition { get; } = new EvDbPartitionAddress("school-records", "students");

    protected override ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentEnlisted enlisted,
        IEvDbEventMeta meta)
    {
        _students.TryAdd(enlisted.Student.Id, enlisted.Student);
        return state;
    }

    protected override ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentReceivedGrade receivedGrade,
        IEvDbEventMeta meta)
    {
        ICollection<StudentScore> topScores = state;
        if (!_students.TryGetValue(receivedGrade.StudentId, out StudentEntity entity))
            throw new Exception("It's broken");
        StudentScore score = new(entity, receivedGrade.Grade);
        IEnumerable<StudentScore> top = [score, .. topScores];
        ICollection<StudentScore> ordered = [.. top.OrderByDescending(x => x.Score).Take(10)];
        return ordered;
    }
}