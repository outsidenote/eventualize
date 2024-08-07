using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Core.Store;

public readonly record struct EvDbSnapshotStoreRegistrationContext(
    EvDbStorageContext? Context,
    EvDbViewBasicAddress Address,
    IServiceCollection Services);

