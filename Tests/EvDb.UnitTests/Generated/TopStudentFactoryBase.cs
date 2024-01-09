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

    protected abstract EvDbPartitionAddress Partition { get; }

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
        // TODO: [bnaya 2024-01-09] review with @Roma
        throw new NotImplementedException();
        EvDbStreamAddress stream = snapshot.Cursor;
        TopStudentAggregate agg =
            new(
                _repository,
                Kind,
                stream,
                this,
                MinEventsBetweenSnapshots,
                DefaultState,
                snapshot.Cursor.Offset,
                JsonSerializerOptions);

        return agg;
    }


    #endregion // Create

    #region GetAsync

    public async Task<ITopStudentAggregate> GetAsync(
        string streamId, long lastStoredOffset = -1, CancellationToken cancellationToken = default)
    {
        EvDbStreamAddress stream = new(Partition, streamId);
        ITopStudentAggregate agg = await _repository.GetAsync(this, stream, cancellationToken);
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
                    var payload = someEvent.GetData<CourseCreated>(JsonSerializerOptions);
                    result = Fold(oldState, payload, someEvent);
                    break;
                }
            case "schedule-test":
                {
                    var payload = someEvent.GetData<ScheduleTest>(JsonSerializerOptions);
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
        CourseCreated payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        ScheduleTest payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentAppliedToCourse payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentCourseApplicationDenied payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentEnlisted payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentQuitCourse payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentReceivedGrade payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentRegisteredToCourse payload,
        IEvDbEventMeta meta) => state;
    protected virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentTestSubmitted payload,
        IEvDbEventMeta meta) => state;

    #endregion // Fold
}
