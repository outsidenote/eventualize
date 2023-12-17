using Generator.Equals;

namespace Eventualize.Core;

// TODO: Json data -> JsonElement if needed at all
[Equatable]
public partial record EventualizeEvent(string EventType, DateTime CapturedAt, string CapturedBy, string JsonData, DateTime? StoredAt)
{
}

