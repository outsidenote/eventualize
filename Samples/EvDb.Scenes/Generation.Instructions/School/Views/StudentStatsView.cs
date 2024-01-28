using EvDb.Core;
using EvDb.Scenes;
using System.Collections.Concurrent;

namespace EvDb.UnitTests;

[EvDbView<StudentStats[], IEvDbSchoolStreamAdders>("student-stats")]
internal partial class StudentStatsView
{
    private readonly ConcurrentDictionary<int, StudentCalc> _students = new();

    protected override StudentStats[] DefaultState { get; } = [];

    public override int MinEventsBetweenSnapshots => 5;

    #region Fold

    protected override StudentStats[] Fold(
        StudentStats[] state,
        StudentEnlistedEvent payload,
        IEvDbEventMeta meta)
    {
        int id = payload.Student.Id;
        string name = payload.Student.Name;
        _students.TryAdd(id,
            new StudentCalc(id, name, 0, 0));
        return state;
    }

    protected override StudentStats[] Fold(
        StudentStats[] state,
        StudentReceivedGradeEvent receivedGrade,
        IEvDbEventMeta meta)
    {
        if (!_students.TryGetValue(receivedGrade.StudentId, out StudentCalc entity))
            throw new Exception("It's broken");

        _students[receivedGrade.StudentId] = entity with
        {
            Count = entity.Count + 1,
            Sum = entity.Sum + receivedGrade.Grade,
        };


        var result = _students.Values
                            .Where(m => m.Count != 0)
                            .Select(m =>
                                new StudentStats(m.StudentName, m.Sum, m.Count));
        return result.ToArray();
    }

    #endregion // Fold
}
