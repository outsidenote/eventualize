using EvDb.Core;
using EvDb.Scenes;
using System.Text.Json;


namespace EvDb.UnitTests;

[EvDbAttachView<StudentStatsView>]
[EvDbAttachView<StatsView>(PropertyName = "ALL")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders>]
public partial class SchoolStreamFactory
{
    #region Ctor

    public SchoolStreamFactory(IEvDbStorageAdapter storageAdapter) : base(storageAdapter)
    {
    }

    #endregion // Ctor

    public override string Kind { get; } = "student-avg";

    #region JsonSerializerOptions

    public override JsonSerializerOptions? JsonSerializerOptions { get; } = SchoolStreamAddersContext.Default.Options;

    #endregion // JsonSerializerOptions

    #region Partition

    public override EvDbPartitionAddress Partition { get; } = new EvDbPartitionAddress("school-records", "students");

    #endregion // Partition
}