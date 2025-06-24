using EvDb.Core;

namespace EvDb.DemoWebApi;

[EvDbDefineEventPayload("created")]
public partial record CreatedEvent(string Name, int Rate);


