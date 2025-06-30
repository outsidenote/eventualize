// Ignore Spelling: Channel

using EvDb.Core;

namespace EvDb.DemoWebApi.Outbox;

[EvDbDefineMessagePayload("rating")]
[EvDbAttachChannel("Rating")]
public partial record RatingMessage(string Id, int rate, DateTimeOffset LastCommentAddedAt);
