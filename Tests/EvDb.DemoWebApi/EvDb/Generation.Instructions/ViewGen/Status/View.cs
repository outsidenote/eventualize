using EvDb.Core;

namespace EvDb.DemoWebApi.Views.Count;

[EvDbViewType<Status, IEvents>("status")]
internal partial class View
{
    protected override Status DefaultState => Status.Empty;

    protected override Status Apply(Status state, CreatedEvent payload, IEvDbEventMeta meta)
    {
        return new Status(
            Name: payload.Name,
            Rate: payload.Rate);
    }

    protected override Status Apply(Status state, ModifiedEvent payload, IEvDbEventMeta meta)
    {
        return state with { Rate = payload.Rate };
    }

    protected override Status Apply(Status state, DeletedEvent payload, IEvDbEventMeta meta)
    {
        return Status.Empty;
    }

    protected override Status Apply(Status state, CommentedEvent payload, IEvDbEventMeta meta)
    {
        return state with
        {
            Comments = state.Comments.Add(payload.Comment)
        };
    }
}
