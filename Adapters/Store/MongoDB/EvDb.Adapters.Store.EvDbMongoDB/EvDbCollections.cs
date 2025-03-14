// Ignore Spelling: Mongo

using MongoDB.Bson;
using MongoDB.Driver;

namespace EvDb.Adapters.Store.MongoDB;

// TODO: [bnaya 2025-02-23] Add OTEL, Logs
// TODO: [bnaya 2025-02-23] replace string with nameof
// TODO: [bnaya 2025-02-23] replace string with nameof
// TODO: [bnaya 2025-03-05] Ensure indexes
// TODO: [bnaya 2025-03-05] Goes away from the _id pattern


public record EvDbCollections(IMongoCollection<BsonDocument> Events,
                              IMongoCollection<BsonDocument> Snapshots);
