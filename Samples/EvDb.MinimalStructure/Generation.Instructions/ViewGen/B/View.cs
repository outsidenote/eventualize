using EvDb.Core;

namespace EvDb.MinimalStructure.Views.B;

[EvDbViewType<int, IEvents>("count")]
internal partial class View
{
    protected override int DefaultState { get; } = 0;

    protected override int Fold(int state, Event2 payload, IEvDbEventMeta meta)
    {
        return state + 1;
    }

    protected override int Fold(int state, Event1 payload, IEvDbEventMeta meta)
    {
        return state + 1;
    }
}
