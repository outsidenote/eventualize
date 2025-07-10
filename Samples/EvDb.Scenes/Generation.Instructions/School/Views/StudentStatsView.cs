using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbViewType<StudentStatsState, IEvDbSchoolStreamAdders>("student-stats")]
internal partial class StudentStatsView
{
    protected override StudentStatsState DefaultState { get; } = StudentStatsState.Empty;

    public override bool ShouldStoreSnapshot(long offsetGapFromLastSave, TimeSpan durationSinceLastSave) => offsetGapFromLastSave > 5;

    #region Apply

    protected override StudentStatsState Apply(
        StudentStatsState state,
        StudentEnlistedEvent payload,
        IEvDbEventMeta meta)
    {
        return state.Add(payload);
    }


    protected override StudentStatsState Apply(
        StudentStatsState state,
        StudentReceivedGradeEvent payload,
        IEvDbEventMeta meta)
    {
        return state.Update(payload);
    }

    #endregion // Append
}
