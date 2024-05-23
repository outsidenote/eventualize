using EvDb.Core;

namespace EvDb.MinimalStructure.Views.B;

[EvDbViewType<State?, IEvents>("a")]
internal partial class View
{
    protected override State? DefaultState { get; } = null;

}
