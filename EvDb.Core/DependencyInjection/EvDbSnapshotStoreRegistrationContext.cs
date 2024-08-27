using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Core.Store.Internals;

/// <summary>
/// Context for the snapshot store registration
/// </summary>
public readonly record struct EvDbSnapshotStoreRegistrationContext(
    EvDbStorageContext? Context,
    EvDbViewBasicAddress Address,
    IServiceCollection Services);

