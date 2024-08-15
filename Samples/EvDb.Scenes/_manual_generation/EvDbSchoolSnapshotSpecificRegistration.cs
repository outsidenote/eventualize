using EvDb.Core;
using EvDb.Core.Store;
using EvDb.Core.Store.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace EvDb.UnitTests.Generated;

/// <summary>
/// Represent the hooking object for specific snapshot storage registration
/// </summary>
public record EvDbSchoolSnapshotSpecificRegistration(
    EvDbStorageContext? Context,
    EvDbViewBasicAddress Address,
    IServiceCollection Services)
{
    /// <summary>
    /// Register a snapshot store provider for `ALL` view
    /// </summary>
    /// <param name="registrationAction">The registration action.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Register a snapshot store provider for `StudentStatsView` view
    /// </summary>
    /// <param name="registrationAction">The registration action.</param>
    /// <returns></returns>
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

