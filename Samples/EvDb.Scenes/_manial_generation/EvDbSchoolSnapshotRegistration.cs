using EvDb.Core;
using EvDb.Core.Store;
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

