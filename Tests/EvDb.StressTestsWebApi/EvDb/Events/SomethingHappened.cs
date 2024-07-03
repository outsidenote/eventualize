using EvDb.Core;

namespace EvDb.StressTestsWebApi;

[EvDbEventPayload("something-happened")]
public partial record SomethingHappened(int Id, string Name);




