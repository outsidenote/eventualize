using EvDb.Core;

namespace EvDb.StressTestsWebApi;

[EvDbDefinePayload("something-happened")]
public partial record SomethingHappened(int Id, string Name);




