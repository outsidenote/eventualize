using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox;

[EvDbDefineMessagePayload("message-2")]
[EvDbAttachChannel("Channel1")]
[EvDbAttachChannel("Channel3")]
public partial record Message2(double Value);