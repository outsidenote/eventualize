using EvDb.Core;
using EvDb.Scenes;
using System.Collections.Immutable;
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

    #region JsonSerializerOptions

    public override JsonSerializerOptions? JsonSerializerOptions { get; } = null; //  SchoolStreamAddersContext.Default.Options;
    // TODO: [bnaya 2024-01-28] use: SchoolStreamAddersContext.Default.Options;

    #endregion // JsonSerializerOptions

    #region Partition

    public override EvDbPartitionAddress PartitionAddress { get; } = new EvDbPartitionAddress("school-records", "students");

    #endregion // PartitionAddress
}