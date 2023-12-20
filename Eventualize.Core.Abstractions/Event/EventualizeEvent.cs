using Generator.Equals;

namespace Eventualize.Core;

// TODO: Json data -> JsonElement if needed at all
[Equatable]
public partial record EventualizeEvent(string EventType,
                                       [property: IgnoreEquality] DateTime CapturedAt,
                                       string CapturedBy,
                                       string JsonData,
                                       [property: IgnoreEquality] DateTime? StoredAt)
{
}

