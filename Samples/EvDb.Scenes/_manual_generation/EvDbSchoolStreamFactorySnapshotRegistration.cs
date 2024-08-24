//using EvDb.Core;
//using EvDb.Core.Store;
//using EvDb.Core.Store.Internals;
//using Microsoft.Extensions.DependencyInjection;

//namespace EvDb.UnitTests.Generated;

//public interface IEvDbSchoolViewRegistrationEntry
//{
//    EvDbPartitionAddress Address { get; }
//    EvDbStorageContext? Context { get; }
//    IServiceCollection Services { get; }
//}

//public readonly record struct EvDbSchoolRegistrationEntry(
//    EvDbStorageContext? Context,
//    EvDbPartitionAddress Address,
//    IServiceCollection Services) : IEvDbSchoolViewRegistrationEntry
//{
//    public EvDbSchoolRegistrationEntry(EvDbStreamStoreRegistrationContext context)
//        : this(context.Context, context.Address, context.Services)
//    {
//    }
//}

///// <summary>
///// Represent the hooking object for snapshot storage registration
///// </summary>
//public static class EvDbSchoolStreamFactorySnapshotRegistration
//{

//    /// <summary>
//    /// Register the defaults snapshot store provider for the snapshot snapshot.
//    /// </summary>
//    /// <param name="registrationAction">The registration action.</param>
//    /// <returns></returns>
//    public static IEvDbSchoolViewRegistrationEntry DefaultSnapshotConfiguration(
//        this EvDbSchoolRegistrationEntry entry,
//        Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
//    {
//        var viewAdress = new EvDbViewBasicAddress(entry.Address, string.Empty);
//        var ctx = new EvDbSnapshotStoreRegistrationContext(
//            entry.Context,
//            viewAdress,
//            entry.Services);

//        registrationAction(ctx);

//        return entry;
//    }

//    /// <summary>
//    /// Register a snapshot store provider for `ALL` view
//    /// </summary>
//    /// <param name="entry"></param>
//    /// <param name="registrationAction">The registration action.</param>
//    /// <returns></returns>
//    public static IEvDbSchoolViewRegistrationEntry ForALL(
//        this EvDbSchoolRegistrationEntry entry,
//        Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
//    {
//        IEvDbSchoolViewRegistrationEntry e = entry;
//        return e.ForALL(registrationAction);
//    }

//    /// <summary>
//    /// Register a snapshot store provider for `StudentStatsView` view
//    /// </summary>
//    /// <param name="entry"></param>
//    /// <param name="registrationAction">The registration action.</param>
//    /// <returns></returns>
//    public static IEvDbSchoolViewRegistrationEntry ForStudentStatsView(
//        this EvDbSchoolRegistrationEntry entry,
//        Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
//    {
//        IEvDbSchoolViewRegistrationEntry e = entry;
//        return e.ForStudentStatsView(registrationAction);
//    }

//    /// <summary>
//    /// Register a snapshot store provider for `ALL` view
//    /// </summary>
//    /// <param name="entry"></param>
//    /// <param name="registrationAction">The registration action.</param>
//    /// <returns></returns>
//    public static IEvDbSchoolViewRegistrationEntry ForALL(
//        this IEvDbSchoolViewRegistrationEntry entry,
//        Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
//    {
//        var viewAdress = new EvDbViewBasicAddress(entry.Address, "All");
//        var ctx = new EvDbSnapshotStoreRegistrationContext(
//            entry.Context,
//            viewAdress,
//            entry.Services);

//        registrationAction(ctx);

//        return entry;
//    }

//    /// <summary>
//    /// Register a snapshot store provider for `StudentStatsView` view
//    /// </summary>
//    /// <param name="entry"></param>
//    /// <param name="registrationAction">The registration action.</param>
//    /// <returns></returns>
//    public static IEvDbSchoolViewRegistrationEntry ForStudentStatsView(
//        this IEvDbSchoolViewRegistrationEntry entry,
//        Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
//    {
//        var viewAdress = new EvDbViewBasicAddress(entry.Address, "StudentStatsView");
//        var ctx = new EvDbSnapshotStoreRegistrationContext(
//            entry.Context,
//            viewAdress,
//            entry.Services);

//        registrationAction(ctx);

//        return entry;
//    }
//}

