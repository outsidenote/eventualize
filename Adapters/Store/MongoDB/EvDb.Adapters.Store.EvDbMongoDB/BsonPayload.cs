﻿namespace EvDb.Adapters.Store.Internals;

internal readonly record struct BsonPayload(string SerializeType, byte[] Payload);
