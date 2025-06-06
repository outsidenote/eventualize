﻿using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbViewType<Stats, IEvDbSchoolStreamAdders>("stats")]
internal partial class StatsView
{
    protected override Stats DefaultState { get; } = new Stats(0, 0);

    public override int MinEventsBetweenSnapshots => 4;

    #region Apply

    protected override Stats Apply(
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

    #endregion // Append
}
