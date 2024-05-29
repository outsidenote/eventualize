using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests.Issues.Views.Count;

[EvDbViewType<int, IEvDbSchoolStreamAdders>("count")]
internal partial class View
{
    protected override int DefaultState { get; } = 0;

    protected override int Fold(int state, CourseCreatedEvent payload, IEvDbEventMeta meta)
    {
        return base.Fold(state + 1, payload, meta);
    }
}
