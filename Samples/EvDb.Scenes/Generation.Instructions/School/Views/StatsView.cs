using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbView<Stats, ISchoolStreamAdders>]
internal partial class StatsView
{
    protected override Stats DefaultState { get; } = new Stats(0, 0);

    public override int MinEventsBetweenSnapshots => 5;

    #region Fold

    protected override Stats Fold(
        Stats state,
        StudentReceivedGradeEvent receivedGrade,
        IEvDbEventMeta meta)
    {
        var result = state with
        {
            Count = state.Count + 1,
            Sum = state.Sum + receivedGrade.Grade,
        };
        return result;
    }

    #endregion // Fold
}
