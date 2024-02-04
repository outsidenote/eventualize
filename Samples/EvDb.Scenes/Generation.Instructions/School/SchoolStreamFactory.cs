using EvDb.Core;


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

    #region JsonSerializerOptions

    //public override JsonSerializerOptions? Options { get; } = SchoolStreamSerializationContext.Default.Options;
    // TODO: [bnaya 2024-01-28] use: SchoolStreamSerializationContext.Default.Options;

    #endregion // JsonSerializerOptions

    #region Partition

    public override EvDbPartitionAddress PartitionAddress { get; } = new EvDbPartitionAddress("school-records", "students");

    #endregion // PartitionAddress
}