namespace EvDb.Adapters.Store.MongoDB.Internals;

internal readonly record struct BsonPayload(string SerializeType, byte[] Payload);
