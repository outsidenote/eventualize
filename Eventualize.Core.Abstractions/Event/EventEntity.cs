﻿using Generator.Equals;

namespace Core;

// TODO: Json data -> JsonElement if needed at all
[Equatable]
public partial record EventEntity (string EventType, DateTime CapturedAt, string CapturedBy, string JsonData, DateTime? StoredAt)
{ 
}
