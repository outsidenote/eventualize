namespace Microsoft.Extensions.DependencyInjection;

using EvDb.Core;

public readonly record struct EvDbStreamStoreRegistrationContext(
    EvDbStorageContext? Context,
    EvDbPartitionAddress Address,
    IServiceCollection Services);

