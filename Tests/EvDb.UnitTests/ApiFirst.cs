using EvDb.Core;
using EvDb.Scenes;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;

namespace EvDb.UnitTests;

public class ApiFirst
{
    [Fact]
    public async Task ApiDesign()
    {
        TopStudentFolding folding = new();
        IEvDbAggregate<ICollection<StudentScore>, IEducationEventTypes> agg = EvDbBuilder.Default
            .AddPartition<IEducationEventTypes>("my-domain", "education")
            .AddAggregateType(folding)
            .AddStreamId("top-users:123")
            .Build();

        var course = new CourseCreated(123, "algorithm", 50);
        agg.Events.Add(course);
    }
}

public class TopStudentFolding : TopStudentAggregateTypeBase
{
    //public TopStudentFolding(): base(this)
    //{

    //}

    private readonly ConcurrentDictionary<int, StudentEntity> _students = new ConcurrentDictionary<int, StudentEntity>();

    public override ICollection<StudentScore> Default { get; } = [];

    public override ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentEnlisted enlisted)
    {
        _students.TryAdd(enlisted.Student.Id, enlisted.Student);
        return state;
    }

    public override ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentReceivedGrade receivedGrade)
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