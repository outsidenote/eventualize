using EvDb.Core;
using EvDb.Core.Store;
using EvDb.Core.Store.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace EvDb.UnitTests.Generated;

/// <summary>
/// Represent the hooking object for snapshot storage registration
/// </summary>
public record EvDbSchoolSnapshotRegistration(
    EvDbStorageContext? Context,
    EvDbPartitionAddress Address,
    IServiceCollection Services)
{
    public EvDbSchoolSnapshotRegistration(EvDbStreamStoreRegistrationContext context)
        : this(context.Context, context.Address, context.Services)
    {
    }

    /// <summary>
    /// Register the defaults snapshot store provider for the snapshot snapshot.
    /// </summary>
    /// <param name="registrationAction">The registration action.</param>
    /// <returns></returns>
    public EvDbSchoolSnapshotSpecificRegistration DefaultSnapshot(
        Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
    {
        var adress = new EvDbViewBasicAddress(Address, string.Empty);
        var ctx = new EvDbSnapshotStoreRegistrationContext(
            Context,
            adress,
            Services);

        registrationAction(ctx);

        return new EvDbSchoolSnapshotSpecificRegistration(Context, adress, Services);
    }

    /// <summary>
    /// Register a snapshot store provider for `ALL` view
    /// </summary>
    /// <param name="registrationAction">The registration action.</param>
    /// <returns></returns>
    public EvDbSchoolSnapshotSpecificRegistration ForALL(
        Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
    {
        var adress = new EvDbViewBasicAddress(Address, "ALL");
        var ctx = new EvDbSnapshotStoreRegistrationContext(
            Context,
            adress,
            Services);

        registrationAction(ctx);

        return new EvDbSchoolSnapshotSpecificRegistration(Context, adress, Services);
    }

    /// <summary>
    /// Register a snapshot store provider for `StudentStatsView` view
    /// </summary>
    /// <param name="registrationAction">The registration action.</param>
    /// <returns></returns>
    public EvDbSchoolSnapshotSpecificRegistration ForStudentStatsView(
        Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
    {
        var adress = new EvDbViewBasicAddress(Address, "StudentStatsView");
        var ctx = new EvDbSnapshotStoreRegistrationContext(
            Context,
            adress,
            Services);

        registrationAction(ctx);

        return new EvDbSchoolSnapshotSpecificRegistration(Context, adress, Services);
    }
}

