using EvDb.Core;
using EvDb.Scenes;
using System.Collections.Immutable;
using System.Text.Json;


namespace EvDb.UnitTests;

[EvDbAggregateFactory<IImmutableDictionary<int, StudentStats>, IStudentFlowEventTypes>]
public partial class SchoolFactory
{
    #region Ctor

    public SchoolFactory(IEvDbStorageAdapter storageAdapter) : base(storageAdapter)
    {
    }

    #endregion // Ctor

    public override string Kind { get; } = "student-avg";

    #region JsonSerializerOptions

    protected override JsonSerializerOptions? JsonSerializerOptions { get; } = StudentFlowEventTypesContext.Default.Options;

    #endregion // JsonSerializerOptions

    #region Partition

    public override EvDbPartitionAddress Partition { get; } = new EvDbPartitionAddress("school-records", "students");

    protected override int MinEventsBetweenSnapshots => 5;

    #endregion // Partition

    #region Fold // deprecate

    protected override IImmutableDictionary<int, StudentStats> DefaultState { get; } = ImmutableDictionary<int, StudentStats>.Empty;

    protected override IImmutableDictionary<int, StudentStats> Fold(
        IImmutableDictionary<int, StudentStats> state,
        StudentEnlistedEvent enlisted,
        IEvDbEventMeta meta)
    {
        int id = enlisted.Student.Id;
        string name = enlisted.Student.Name;
        if (!state.ContainsKey(id))
        {
            state = state.Add(id, new StudentStats(name, 0, 0));
        }
        return state;
    }

    protected override IImmutableDictionary<int, StudentStats> Fold(
        IImmutableDictionary<int, StudentStats> state,
        StudentReceivedGradeEvent receivedGrade,
        IEvDbEventMeta meta)
    {
        int id = receivedGrade.StudentId;
        if (!state.ContainsKey(id))
            throw new KeyNotFoundException($"{id}");

        var item = state[id]
                    .AddGrade(receivedGrade.Grade);
        state = state.SetItem(id, item);
        return state;
    }

    #endregion // Fold // deprecate
}
