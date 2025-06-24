// Ignore Spelling: Channel

using Avro;
using Avro.Specific;
using EvDb.Core;

namespace EvDb.DemoWebApi.Outbox;

[EvDbDefineMessagePayload("comments")]
[EvDbAttachChannel("Comments")]
public partial record CommentsMessage(string Id, string[] Comments, DateTimeOffset LastCommentAddedAt);
