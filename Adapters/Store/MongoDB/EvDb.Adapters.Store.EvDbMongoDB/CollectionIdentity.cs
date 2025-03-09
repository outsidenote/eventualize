namespace EvDb.Adapters.Store.EvDbMongoDB.Internals;

internal readonly record struct CollectionIdentity(string DatabaseName, string CollectionName)
{
    public override string ToString() => $"{DatabaseName}.{CollectionName}";
}
