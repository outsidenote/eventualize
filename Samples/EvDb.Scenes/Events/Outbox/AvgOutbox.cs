// Ignore Spelling: Outbox

using EvDb.Core;

namespace EvDb.Scenes;

//[EvDbTopic("topic-2")]
//[EvDbTopic("topic-1")]
[EvDbPayload("avg")]
public partial record AvgOutbox(double Avg);



