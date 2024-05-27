using EvDb.Core;

namespace EvDb.MinimalStructure.Views.A;

[EvDbViewType<State?, IEvents>("a")]
internal partial class View
{
    protected override State? DefaultState { get; } = null;

    protected override State? Fold(State? state, Event1 payload, IEvDbEventMeta meta)
    {
        return base.Fold(state, payload, meta);
    }

    protected override State? Fold(State? state, Event2 payload, IEvDbEventMeta meta)
    {
        return base.Fold(state, payload, meta);
    }
}
