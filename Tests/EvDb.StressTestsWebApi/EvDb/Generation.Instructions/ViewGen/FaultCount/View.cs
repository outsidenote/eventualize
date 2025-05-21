using EvDb.Core;

namespace EvDb.StressTestsWebApi.Views.FaultCount;

[EvDbViewType<int, IEvents>("fault-count")]
internal partial class View
{
    protected override int DefaultState { get; } = 0;

    protected override int Apply(int state, FaultOccurred payload, IEvDbEventMeta meta) => state + 1;
}
