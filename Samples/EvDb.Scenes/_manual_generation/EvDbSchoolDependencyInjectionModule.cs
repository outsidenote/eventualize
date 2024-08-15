using EvDb.Core;
using EvDb.Core.Internals;
using EvDb.Core.Store;
using EvDb.UnitTests;
using EvDb.UnitTests.Generated;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Specialize registration for the School event stream
/// </summary>
public static class EvDbSchoolDependencyInjectionModule
{
    /// <summary>
    /// Specialize the store registration for the School event stream
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="registrationAction">The registration action.</param>
    /// <param name="context">The context</param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public static EvDbSchoolSnapshotRegistration AddSchool(
        this EvDbRegistrationEntry instance,
        Action<EvDbStreamStoreRegistrationContext> registrationAction,
        EvDbStorageContext? context = null)
    {
        IServiceCollection services = instance.Services;
        services.AddKeyedScoped<IEvDbSchoolStreamFactory, SchoolStreamFactory> ("school-records:students");
        services.AddKeyedScoped<IEvDbViewFactory, EvDb.UnitTests.StatsViewFactory> ("school-records:students:ALL");
        services.AddKeyedScoped<IEvDbViewFactory, EvDb.UnitTests.StudentStatsViewFactory> ("school-records:students:StudentStatsView");
        services.AddEvDbSchoolStreamViewsFactories();


        var storageContext = new EvDbStreamStoreRegistrationContext(context,
            new EvDbPartitionAddress("school-records", "students"),
            services);
        registrationAction(storageContext);
        var viewContext = new EvDbSchoolSnapshotRegistration(
                                storageContext.Context, 
                                storageContext.Address,
                                services);
        return viewContext;
    }
}

