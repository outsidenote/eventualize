using EvDb.Core;

namespace EvDb.UnitTests.Issues.Views.B;

[EvDbViewType<State?, IEvDbSchoolStreamAdders>("a")]
internal partial class View
{
    protected override State? DefaultState { get; } = null;
}
