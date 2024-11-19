using EvDb.Core;

namespace EvDb.StressTests;

[EvDbDefineEventPayload("something-happened")]
public partial record SomethingHappened(int Id, string Name);




