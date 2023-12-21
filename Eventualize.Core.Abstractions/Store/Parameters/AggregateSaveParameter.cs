using System.Diagnostics;

namespace Eventualize.Core;

[DebuggerDisplay("{AggregateId}, {AggregateType}, {EventType}, {Sequence}")]
public readonly record struct AggregateSaveParameter(
                    string AggregateId,
                    string AggregateType,
                    string EventType,
                    long Sequence,
                    // TODO: [bnaya 2023-12-20] use IPayload
                    string Payload,
                    string CapturedBy,
                    // TODO: [bnaya 2023-12-20] datetime offset
                    DateTime CapturedAt,
                    string Domain);