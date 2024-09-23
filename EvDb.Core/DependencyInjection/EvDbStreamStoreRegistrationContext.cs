namespace Microsoft.Extensions.DependencyInjection;

using EvDb.Core;
using EvDb.Core.Store;

public readonly record struct EvDbStreamStoreRegistrationContext(
    EvDbStorageContext? Context,
    EvDbPartitionAddress Address,
    IServiceCollection Services);

