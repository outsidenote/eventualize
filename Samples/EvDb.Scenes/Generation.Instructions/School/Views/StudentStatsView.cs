using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbViewType<StudentStatsState, IEvDbSchoolStreamAdders>("student-stats")]
internal partial class StudentStatsView
{
    protected override StudentStatsState DefaultState { get; } = StudentStatsState.Empty;

    public override int MinEventsBetweenSnapshots => 5;

    #region Fold

    protected override StudentStatsState Fold(
        StudentStatsState state,
        StudentEnlistedEvent payload,
        IEvDbEventMeta meta)
    {
        return state.Add(payload);
    }


    protected override StudentStatsState Fold(
        StudentStatsState state,
        StudentReceivedGradeEvent payload,
        IEvDbEventMeta meta)
    {
        return state.Update(payload);
    }

    #endregion // Fold
}
