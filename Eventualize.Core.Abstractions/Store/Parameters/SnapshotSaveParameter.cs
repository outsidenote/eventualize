using System.Diagnostics;

namespace Eventualize.Core;

[DebuggerDisplay("{AggregateId}, {AggregateType}, {Sequence}")]
public readonly record struct SnapshotSaveParameter(
                    string AggregateId,
                    string AggregateType,
                    long Sequence,
                    // TODO: [bnaya 2023-12-20] use ISnapshotPayload
                    string Payload,
                    string Domain);
