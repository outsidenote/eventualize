using EvDb.Core;

namespace EvDb.DemoWebApi;


[EvDbAttachEventType<CreatedEvent>]
[EvDbAttachEventType<ModifiedEvent>]
[EvDbAttachEventType<DeletedEvent>]
[EvDbAttachEventType<CommentedEvent>]
public partial interface IEvents
{
}

