using EvDb.Core;
using EvDb.Scenes;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.UnitTests;

public interface ITopStudentAggregate : IEvDbAggregate<ICollection<StudentScoreState>>, IEducationEventTypes
{
}

[GeneratedCode("The following line should generated", "v0")]
[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public class TopStudentAggregate : EvDbAggregate<ICollection<StudentScoreState>>,
    ITopStudentAggregate
{
    private readonly JsonSerializerOptions? _options;

    #region Ctor

    public TopStudentAggregate(
        IEvDbRepository repository,
        string kind,
        EvDbStreamAddress streamId,
        IEvDbFoldingLogic<ICollection<StudentScoreState>> foldingLogic,
        int minEventsBetweenSnapshots,
        ICollection<StudentScoreState> state,
        long lastStoredOffset,
        JsonSerializerOptions? options) : base(repository, kind, streamId, foldingLogic, minEventsBetweenSnapshots, state, lastStoredOffset, options)
    {
        _options = options;
    }

    #endregion // Ctor

    #region Add

    void IEducationEventTypes.Add(CourseCreatedEvent payload, string? capturedBy = null)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(ScheduleTestEvent payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentAppliedToCourseEvent payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentCourseApplicationDeniedEvent payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentEnlistedEvent payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentQuitCourseEvent payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentReceivedGradeEvent payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentRegisteredToCourseEvent payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    void IEducationEventTypes.Add(StudentTestSubmittedEvent payload, string? capturedBy)
    {
        AddEvent(payload, capturedBy);
    }

    #endregion // Add
}
