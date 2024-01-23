using EvDb.Core;
using EvDb.Scenes;
using System.Text.Json;


namespace EvDb.UnitTests;

[EvDbViewRef<StudentStatsView>]
[EvDbViewRef<StatsView>(PropertyName = "ALL")]
[EvDbStreamFactory<ISchoolEventBundle>]
public partial class SchoolXFactory
{
    #region Ctor

    public SchoolXFactory(IEvDbStorageAdapter storageAdapter) : base(storageAdapter)
    {
    }

    #endregion // Ctor

    public override string Kind { get; } = "student-avg";

    #region JsonSerializerOptions

    public override JsonSerializerOptions? JsonSerializerOptions { get; } = SchoolEventBundleContext.Default.Options;

    #endregion // JsonSerializerOptions

    #region Partition

    public override EvDbPartitionAddress Partition { get; } = new EvDbPartitionAddress("school-records", "students");

    #endregion // Partition
}