﻿using EvDb.Core;
using EvDb.Scenes;
using System.Collections.Concurrent;

namespace EvDb.UnitTests;

[EvDbView<IEnumerable<StudentStats>, ISchoolStreamAdders>]
internal partial class StudentStatsView
{
    private readonly ConcurrentDictionary<int, StudentCalc> _students = new();

    protected override IEnumerable<StudentStats> DefaultState { get; } = [];

    public override int MinEventsBetweenSnapshots => 5;

    #region Fold

    protected override IEnumerable<StudentStats> Fold(
        IEnumerable<StudentStats> state,
        StudentEnlistedEvent enlisted,
        IEvDbEventMeta meta)
    {
        int id = enlisted.Student.Id;
        string name = enlisted.Student.Name;
        _students.TryAdd(id,
            new StudentCalc(id, name, 0, 0));
        return state;
    }

    protected override IEnumerable<StudentStats> Fold(
        IEnumerable<StudentStats> state,
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
