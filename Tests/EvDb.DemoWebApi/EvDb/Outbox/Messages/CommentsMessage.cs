// Ignore Spelling: Channel

using EvDb.Core;

namespace EvDb.DemoWebApi.Outbox;

[EvDbDefineMessagePayload("comments")]
[EvDbAttachChannel("Comments")]
public partial record CommentsMessage(string Id, string[] Comments, DateTimeOffset LastCommentAddedAt);
