using EvDb.Core;

namespace EvDb.StressTests.Outbox;

[EvDbDefineMessagePayload("message-2")]
[EvDbAttachChannel("Channel1")]
[EvDbAttachChannel("Channel3")]
public partial record Message2(double Value);