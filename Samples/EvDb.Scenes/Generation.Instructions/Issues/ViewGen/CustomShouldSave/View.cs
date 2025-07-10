using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests.Issues.Views.CustomShouldSave;

[EvDbViewType<State?, IEvDbSchoolStreamAdders>("custom-should-save")]
internal partial class View
{
    private bool _enforceSave = false;

    protected override State? DefaultState { get; } = default;

    public override bool ShouldStoreSnapshot(long offsetGapFromLastSave, TimeSpan durationSinceLastSave)
    {
        if (_enforceSave)
        {
            _enforceSave = false;
            return true;
        }

        return offsetGapFromLastSave > 5;
    }

    protected override State? Apply(State? state, StudentEnlistedEvent payload, IEvDbEventMeta meta)
    {
        _enforceSave = true;
        return new State(meta.EventType, meta.StreamCursor.Offset);
    }

    protected override State? Apply(
        State? state,
        StudentReceivedGradeEvent payload,
        IEvDbEventMeta meta)
    {
        return new State(meta.EventType, meta.StreamCursor.Offset);
    }
}
