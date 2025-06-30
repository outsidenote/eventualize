// Ignore Spelling: Channel

using EvDb.Core;

namespace EvDb.DemoWebApi.Outbox;

[EvDbDefineMessagePayload("liveness")]
[EvDbAttachChannel("Lifetime")]
public partial record LivenessMessage(string Id, bool IsAlive, DateTimeOffset LastCommentAddedAt);
