using EvDb.Core;
using EvDb.Core.Store;
using EvDb.UnitTests.Generated;
using Microsoft.Extensions.DependencyInjection.Internals;

namespace Microsoft.Extensions.DependencyInjection;

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
        var storageContext = new EvDbStreamStoreRegistrationContext(context,
            new EvDbPartitionAddress("school-records", "students"),
            instance.Services);
        registrationAction(storageContext);
        var viewContext = new EvDbSchoolSnapshotRegistration(
                                storageContext.Context, 
                                storageContext.Address,
                                storageContext.Services);
        return viewContext;
    }
}

