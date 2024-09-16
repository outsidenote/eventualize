using EvDb.Core;

namespace EvDb.StressTestsWebApi;

[EvDbPayload("something-happened")]
public partial record SomethingHappened(int Id, string Name);




