using EvDb.Core;


namespace EvDb.UnitTests;

[EvDbAttachView<StudentStatsView>]
[EvDbAttachView<StatsView>("ALL")]
[EvDbAttachView<MinEventIntervalSecondsView>("MinInterval")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders>("school-records", "students")]
public partial class SchoolStreamFactory
{
    #region JsonSerializerOptions

    //public override JsonSerializerOptions? Options { get; } = SchoolStreamSerializationContext.Default.Options;
    // TODO: [bnaya 2024-01-28] use: SchoolStreamSerializationContext.Default.Options;

    #endregion // JsonSerializerOptions
}