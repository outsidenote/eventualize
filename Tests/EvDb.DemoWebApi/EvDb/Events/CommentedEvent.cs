using EvDb.Core;

namespace EvDb.DemoWebApi;

[EvDbDefineEventPayload("commented")]
public partial record CommentedEvent(string Comment);



