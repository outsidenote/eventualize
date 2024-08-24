//#nullable enable
//#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
//#pragma warning disable CS0105 // Using directive appeared previously in this namespace
//#pragma warning disable CS0108 // hides inherited member.

//using System.Collections.Immutable;
//using System.Text.Json;
//// ####################  GENERATED AT: 2024-08-20 12:24:31 ####################
//using EvDb.Core;
//namespace Microsoft.Extensions.DependencyInjection;
//using EvDb.UnitTests;
//using EvDb.UnitTests.Generated;
//using EvDb.Core.Internals;
//using EvDb;

//public static class EvDbSchoolStreamFactoryRegistration
//{
//    public static EvDbSchoolRegistrationEntry AddSchoolStreamFactory(
//        this EvDbRegistrationEntry instance,
//        Action<EvDbStreamStoreRegistrationContext> registrationAction,
//        EvDbStorageContext? context = null)
//    {
//        IServiceCollection services = instance.Services;
//        services.AddEvDbSchoolStreamViewsFactories();

//        services.AddKeyedScoped<IEvDbViewFactory, EvDb.UnitTests.StudentStatsViewFactory>("school-records:students:StudentStats");
//        services.AddKeyedScoped<IEvDbViewFactory, EvDb.UnitTests.StatsViewFactory>("school-records:students:ALL");


//        services.AddScoped<IEvDbSchoolStreamFactory, SchoolStreamFactory>();

//        var storageContext = new EvDbStreamStoreRegistrationContext(context,
//            new EvDbPartitionAddress("school-records", "students"),
//            services);

//        registrationAction(storageContext);
//        var viewContext = new EvDbSchoolRegistrationEntry(storageContext);
//        return viewContext;
//    }
//}
