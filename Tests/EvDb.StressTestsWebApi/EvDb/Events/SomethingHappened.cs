using EvDb.Core;

namespace EvDb.StressTestsWebApi;

[EvDbDefineEventPayload("something-happened")]
public partial record SomethingHappened(int Id, string Name);




