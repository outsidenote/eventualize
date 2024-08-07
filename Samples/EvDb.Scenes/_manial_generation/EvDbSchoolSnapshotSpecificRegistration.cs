using EvDb.Core;
using EvDb.Core.Store;
using Microsoft.Extensions.DependencyInjection;

namespace EvDb.UnitTests.Generated;

public record EvDbSchoolSnapshotSpecificRegistration(
    EvDbStorageContext? Context,
    EvDbViewBasicAddress Address,
    IServiceCollection Services)
{
    public EvDbSchoolSnapshotSpecificRegistration ForALL(
        Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
    {
        var adress = Address with { ViewName = "ALL" };
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
        var adress = Address with { ViewName = "StudentStatsView" };
        var ctx = new EvDbSnapshotStoreRegistrationContext(
            Context,
            adress,
            Services);

        registrationAction(ctx);

        return new EvDbSchoolSnapshotSpecificRegistration(Context, adress, Services);
    }
}

