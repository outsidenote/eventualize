using EvDb.Core;
using EvDb.Scenes;
using System.Collections.Immutable;

namespace EvDb.UnitTests.Issues.Views.CourceCreated;

[EvDbViewType<IImmutableList<CourseCreatedEvent>, IEvDbSchoolStreamAdders>("courses")]
internal partial class View
{
    protected override IImmutableList<CourseCreatedEvent> DefaultState { get; } = ImmutableList<CourseCreatedEvent>.Empty;

    protected override IImmutableList<CourseCreatedEvent> Apply(IImmutableList<CourseCreatedEvent> state, CourseCreatedEvent payload, IEvDbEventMeta meta)
    {
        return base.Apply(state.Add(payload), payload, meta);
    }
}
