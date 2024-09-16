using EvDb.Core;

namespace EvDb.StressTestsWebApi;

[EvDbPayload("fault-occurred")]
public partial record FaultOccurred(int Id, string Name, int Rate);



