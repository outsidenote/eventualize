using EvDb.Core;
using EvDb.Scenes;
using System.Collections.Concurrent;
using System.Text.Json;

namespace EvDb.UnitTests;

[EvDbAggregateFactory<ICollection<StudentScoreState>, IEducationEventTypes>]
public partial class TopStudentFactory
{
    private readonly ConcurrentDictionary<int, StudentEntity> _students = new ConcurrentDictionary<int, StudentEntity>();

    protected override ICollection<StudentScoreState> DefaultState { get; } = new List<StudentScoreState>();

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
        IEnumerable<StudentScoreState> top = new[] { score }.Concat(topScores);
        ICollection<StudentScoreState> ordered = top.OrderByDescending(x => x.Score).Take(10).ToList();
        return ordered;
    }
}