using EvDb.Core;

namespace EvDb.UnitTests;

[EvDbAttachView<StudentStatsView>]
[EvDbAttachView<StatsView>("ALL")]
//[EvDbAttachView<MinEventIntervalSecondsView>("MinInterval")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders, EvDbSchoolOutbox>(
                        "school-records1", "students")]
// Lifetime = ServiceLifetime.Scoped)]
public partial class SchoolStreamFactory
{
}

