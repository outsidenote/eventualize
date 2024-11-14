// Ignore Spelling: Channel

using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox;

[EvDbDefineMessagePayload("message-1")]
[EvDbAttachChannel("Channel1")]
[EvDbAttachChannel("Channel2")]
public partial record Message1(double Value);