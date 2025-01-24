using EvDb.Core;
using EvDb.IntegrationTests.EF;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public class EvDbPersonStorageStreamAdapterFactory : IEvDbTypedSnapshotStorageAdapterFactory
{
    private readonly IDbContextFactory<PersonContext> _contextFactory;

    public EvDbPersonStorageStreamAdapterFactory(IDbContextFactory<PersonContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    IEvDbTypedStorageSnapshotAdapter IEvDbTypedSnapshotStorageAdapterFactory.Create(
                                                    IEvDbStorageSnapshotAdapter adapter,
                                                    Predicate<EvDbViewAddress>? canHandle)
    {
        return new EvDbPersonStorageStreamAdapter(_contextFactory, adapter, canHandle);
    }
}
