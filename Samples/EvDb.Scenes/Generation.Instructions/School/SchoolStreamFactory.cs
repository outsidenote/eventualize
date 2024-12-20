﻿using EvDb.Core;

namespace EvDb.UnitTests;

[EvDbAttachView<StudentStatsView>]
[EvDbAttachView<StatsView>("ALL")]
//[EvDbAttachView<MinEventIntervalSecondsView>("MinInterval")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders, EvDbSchoolOutbox>("school-records", "students")]
public partial class SchoolStreamFactory
{
}

