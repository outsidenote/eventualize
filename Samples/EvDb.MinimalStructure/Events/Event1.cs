using EvDb.Core;

namespace EvDb.MinimalStructure;

[EvDbDefineEventPayload("event-1")]
public partial record Event1(int Id, string Name, int Capacity);



