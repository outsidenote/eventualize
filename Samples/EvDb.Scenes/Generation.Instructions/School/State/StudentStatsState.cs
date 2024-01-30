using EvDb.Scenes;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace EvDb.Scenes;

[DebuggerDisplay("Count: {Students.Count}")]
public record StudentStatsState 
{
    public static readonly StudentStatsState Empty = new StudentStatsState();

    public IImmutableList<StudentStats> Students { get; init; } = ImmutableList<StudentStats>.Empty;

    #region Add

    public StudentStatsState Add(StudentEnlistedEvent payload)
    {
        var student = payload.Student;
        int id = student.Id;
        var newStudent = new StudentStats(id, student.Name, 0, 0);
        var students = Students.Add(newStudent);
        return this with { Students = students };
    }

    #endregion // Add

    #region Update

    public StudentStatsState Update(StudentReceivedGradeEvent receivedGrade)
    {
        int id = receivedGrade.StudentId;
        StudentStats v = Students.First();
        var students = Students
                            .Remove(v)
                            .Add(v with
                                {
                                    Count = v.Count + 1,
                                    Sum = v.Sum + receivedGrade.Grade,
                                });
        return this with { Students = students };
    }

    #endregion // Update
}
