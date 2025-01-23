using EvDb.Core;

namespace EvDb.MinimalStructure;

[EvDbDefineEventPayload("sample")]
public readonly partial record struct EventSample(int Value)
{
}
