using EvDb.Core;

namespace EvDb.StressTestsWebApi.Views.A;

[EvDbViewType<State?, IEvents>("a")]
internal partial class View
{
    protected override State? DefaultState { get; } = null;

    protected override State? Fold(State? state, FaultOccurred payload, IEvDbEventMeta meta)
    {
        return base.Fold(state, payload, meta);
    }

    protected override State? Fold(State? state, SomethingHappened payload, IEvDbEventMeta meta)
    {
        return base.Fold(state, payload, meta);
    }
}
