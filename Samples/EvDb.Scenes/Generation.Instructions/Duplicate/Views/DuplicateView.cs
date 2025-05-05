using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbViewType<Stats, IEvDbSchoolStreamAdders>("duplicate")]
internal partial class DuplicateView
{
    protected override Stats DefaultState { get; } = new Stats(0, 0);

    public override int MinEventsBetweenSnapshots => 4;

    #region Fold

    protected override Stats Fold(
        Stats state,
        StudentReceivedGradeEvent payload,
        IEvDbEventMeta meta)
    {
        var result = state with
        {
            Count = state.Count + 1,
            Sum = state.Sum + payload.Grade,
        };
        return result;
    }

    #endregion // Fold
}
