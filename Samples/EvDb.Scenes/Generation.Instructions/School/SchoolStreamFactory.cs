﻿using EvDb.Core;


namespace EvDb.UnitTests;

[EvDbAttachView<StudentStatsView>]
[EvDbAttachView<StatsView>("ALL")]
//[EvDbAttachView<MinEventIntervalSecondsView>("MinInterval")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders>]
public partial class SchoolStreamFactory
{
    #region JsonSerializerOptions

    //public override JsonSerializerOptions? Options { get; } = SchoolStreamSerializationContext.Default.Options;
    // TODO: [bnaya 2024-01-28] use: SchoolStreamSerializationContext.Default.Options;

    #endregion // JsonSerializerOptions

    #region Partition

    public override EvDbPartitionAddress PartitionAddress { get; } = new EvDbPartitionAddress("school-records", "students");

    #endregion // PartitionAddress
}