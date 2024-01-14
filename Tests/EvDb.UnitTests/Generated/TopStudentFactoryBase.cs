using EvDb.Core;
using EvDb.Scenes;
using System.CodeDom.Compiler;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text.Json;
using static EvDb.UnitTests.TopStudentFactory;

namespace EvDb.UnitTests;

[DebuggerDisplay("{Kind}...")]
[GeneratedCode("from IEducationEventTypes<T0, T1,...>", "v0")]
public abstract class TopStudentFactoryBase: 
        AggregateFactoryBase<ITopStudentAggregate, ICollection<StudentScoreState>>
{
    #region Ctor

    public TopStudentFactoryBase(IEvDbStorageAdapter storageAdapter) :base(storageAdapter)
    {
    }

    #endregion // Ctor

    #region Create

    public override ITopStudentAggregate Create(
        string streamId, 
        long lastStoredOffset = -1)
    {
        EvDbStreamAddress stream = new(Partition, streamId);
        TopStudentAggregate agg =
            new(
                _repository,
                Kind,
                stream,
                this,
                MinEventsBetweenSnapshots,
                DefaultState,
                lastStoredOffset,
                JsonSerializerOptions);

        return agg;
    }

    public override ITopStudentAggregate Create(EvDbStoredSnapshot<ICollection<StudentScoreState>> snapshot)
    {
        EvDbStreamAddress stream = snapshot.Cursor;
        TopStudentAggregate agg =
            new(
                _repository,
                Kind,
                stream,
                this,
                MinEventsBetweenSnapshots,
                snapshot.State,
                snapshot.Cursor.Offset,
                JsonSerializerOptions);

        return agg;
    }


    #endregion // Create
 
    #region FoldEvent

    protected override ICollection<StudentScoreState> FoldEvent(
        ICollection<StudentScoreState> oldState, 
        IEvDbEvent someEvent)
    {
        ICollection<StudentScoreState> result;
        switch (someEvent.EventType)
        {
            case  "course-created":
                {
                    var payload = someEvent.GetData<CourseCreatedEvent>(JsonSerializerOptions);
                    result = Fold(oldState, payload, someEvent);
                    break;
                }
            case "schedule-test":
                {
                    var payload = someEvent.GetData<ScheduleTestEvent>(JsonSerializerOptions);
                    result = Fold(oldState, payload, someEvent);
                    break;
                }

                // TODO: others ...
            default:
                throw new NotSupportedException(someEvent.EventType);
        }
        return result;  
    }

    #endregion // FoldEvent

    #region Fold

    protected virtual ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        CourseCreatedEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        ScheduleTestEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        StudentAppliedToCourseEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        StudentCourseApplicationDeniedEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        StudentEnlistedEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        StudentQuitCourseEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        StudentReceivedGradeEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        StudentRegisteredToCourseEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScoreState> Fold(
        ICollection<StudentScoreState> state,
        StudentTestSubmittedEvent payload,
        IEvDbEventMeta meta) => state;

    #endregion // Fold
}
