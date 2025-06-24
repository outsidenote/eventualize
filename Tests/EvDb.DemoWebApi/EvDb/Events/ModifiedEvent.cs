using EvDb.Core;

namespace EvDb.DemoWebApi;

[EvDbDefineEventPayload("modified")]
public partial record ModifiedEvent(int Rate);



