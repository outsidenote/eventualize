using EvDb.Core;
using EvDb.Scenes;
using FakeItEasy;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EvDb.UnitTests;

[JsonSerializable(typeof(CourseCreatedEvent))]
[JsonSerializable(typeof(ScheduleTestEvent))]
[JsonSerializable(typeof(StudentAppliedToCourseEvent))]
[JsonSerializable(typeof(StudentCourseApplicationDeniedEvent))]
[JsonSerializable(typeof(StudentEnlistedEvent))]
[JsonSerializable(typeof(StudentQuitCourseEvent))]
[JsonSerializable(typeof(StudentReceivedGradeEvent))]
[JsonSerializable(typeof(StudentRegisteredToCourseEvent))]
[JsonSerializable(typeof(StudentTestSubmittedEvent))]
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
        var course = new CourseCreatedEvent(123, "algorithm", 50);
        agg.Add(course);
    }
}

// TODO: [bnaya 2024-01-07] should be a factory
[EvDbAggregateFactory<ICollection<StudentScoreState>, IEducationEventTypes>]
public partial class TopStudentFactory 
{    
    private readonly ConcurrentDictionary<int, StudentEntity> _students = new ConcurrentDictionary<int, StudentEntity>();

    protected override ICollection<StudentScoreState> DefaultState { get; } = [];

    public override string Kind { get; } = "top-student";

    protected override JsonSerializerOptions? JsonSerializerOptions { get; } = EducationEventTypesContext.Default.Options;

    public override EvDbPartitionAddress Partition { get; } = new EvDbPartitionAddress("school-records", "students");

    protected override ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        StudentEnlistedEvent enlisted,
        IEvDbEventMeta meta)
    {
        _students.TryAdd(enlisted.Student.Id, enlisted.Student);
        return state;
    }

    protected override ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        StudentReceivedGradeEvent receivedGrade,
        IEvDbEventMeta meta)
    {
        ICollection<StudentScoreState> topScores = state;
        if (!_students.TryGetValue(receivedGrade.StudentId, out StudentEntity entity))
            throw new Exception("It's broken");
        StudentScoreState score = new(entity, receivedGrade.Grade);
        IEnumerable<StudentScoreState> top = [score, .. topScores];
        ICollection<StudentScoreState> ordered = [.. top.OrderByDescending(x => x.Score).Take(10)];
        return ordered;
    }
}