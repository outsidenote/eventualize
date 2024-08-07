using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Core.Store;

public readonly record struct EvDbStreamStoreRegistrationContext(
    EvDbStorageContext? Context,
    EvDbPartitionAddress Address,
    IServiceCollection Services);

