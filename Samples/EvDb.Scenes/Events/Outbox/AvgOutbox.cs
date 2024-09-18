// Ignore Spelling: Outbox

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("avg")]
public partial record AvgOutbox(double Avg);



