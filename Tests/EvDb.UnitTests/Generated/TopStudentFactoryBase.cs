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
public abstract class TopStudentFactoryBase: IEvDbFoldingLogic<ICollection<StudentScore>>,
        IEvDbAggregateFactory<ITopStudentAggregate, ICollection<StudentScore>>
{
    private readonly IEvDbRepository _repository;

    protected abstract ICollection<StudentScore> DefaultState { get; }

    public abstract string Kind { get; } 

    public abstract EvDbPartitionAddress Partition { get; }

    #region Ctor

    public TopStudentFactoryBase(IEvDbStorageAdapter storageAdapter)
    {
        _repository = new EvDbRepository(storageAdapter);
    }

    #endregion // Ctor

    protected virtual int MinEventsBetweenSnapshots{ get; }

    protected virtual JsonSerializerOptions? JsonSerializerOptions { get; }

    #region Create

    public ITopStudentAggregate Create(
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

    public ITopStudentAggregate Create(EvDbStoredSnapshot<ICollection<StudentScore>> snapshot)
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

    #region GetAsync

    public async Task<ITopStudentAggregate> GetAsync(
        string streamId, long lastStoredOffset = -1, CancellationToken cancellationToken = default)
    {
        ITopStudentAggregate agg = await _repository.GetAsync(this, streamId, cancellationToken);
        return agg;
    }

    #endregion // GetAsync

    #region FoldEvent

    ICollection<StudentScore> IEvDbFoldingLogic<ICollection<StudentScore>>.FoldEvent(
        ICollection<StudentScore> oldState, 
        IEvDbEvent someEvent)
    {
        ICollection<StudentScore> result;
        switch (someEvent.EventType)
        {
            case "course-created":
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

    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        CourseCreatedEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        ScheduleTestEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentAppliedToCourseEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentCourseApplicationDeniedEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentEnlistedEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentQuitCourseEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentReceivedGradeEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentRegisteredToCourseEvent payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentTestSubmittedEvent payload,
        IEvDbEventMeta meta) => state;

    #endregion // Fold
}
