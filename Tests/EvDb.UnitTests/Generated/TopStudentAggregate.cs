using EvDb.Core;
using EvDb.Scenes;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace EvDb.UnitTests;

public interface ITopStudentAggregate : IEvDbAggregate<ICollection<StudentScore>>, IEducationEventTypes, IEvDbEventPublisher
{ 
}

[GeneratedCode("The following line should generated", "v0")]
[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public class TopStudentAggregate : EvDbAggregate<ICollection<StudentScore>>,
    ITopStudentAggregate
{
    private readonly JsonSerializerOptions? _options;

    #region Ctor

    public TopStudentAggregate(
        IEvDbRepository repository,
        string kind,
        EvDbStreamAddress streamId,
        IEvDbFoldingLogic<ICollection<StudentScore>> foldingLogic,
        int minEventsBetweenSnapshots,
        ICollection<StudentScore> state,
        long lastStoredOffset,
        JsonSerializerOptions? options) : base(repository, kind, streamId, foldingLogic, minEventsBetweenSnapshots, state, lastStoredOffset, options)
    {
        _options = options;
    }

    #endregion // Ctor

    #region Add

    void IEducationEventTypes.Add(CourseCreated payload, string? capturedBy = null)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(ScheduleTest payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentAppliedToCourse payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentCourseApplicationDenied payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentEnlisted payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentQuitCourse payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentReceivedGrade payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentRegisteredToCourse payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentTestSubmitted payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    #endregion // Add
}
