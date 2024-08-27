using EvDb.Core;

namespace EvDb.UnitTests.Issues.Views.A;

[EvDbViewType<State?, IEvDbSchoolStreamAdders>("a")]
internal partial class View
{
    protected override State? DefaultState { get; } = default;
}
