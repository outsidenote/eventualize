using EvDb.Core;

namespace EvDb.MinimalStructure;

[EvDbPayload("event-1")]
public partial record Event1(int Id, string Name, int Capacity);



