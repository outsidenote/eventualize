using EvDb.Core;
using EvDb.IntegrationTests.EF;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public class EvDbPerson2StorageStreamAdapterFactory : IEvDbTypedSnapshotStorageAdapterFactory
{
    private readonly IDbContextFactory<Person2Context> _contextFactory;

    public EvDbPerson2StorageStreamAdapterFactory(IDbContextFactory<Person2Context> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    IEvDbTypedStorageSnapshotAdapter IEvDbTypedSnapshotStorageAdapterFactory.Create(
                                                    IEvDbStorageSnapshotAdapter adapter,
                                                    Predicate<EvDbViewAddress>? canHandle)
    {
        return new EvDbPerson2StorageStreamAdapter(_contextFactory, adapter, canHandle);
    }
}
