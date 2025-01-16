using EvDb.Core;
using Microsoft.Extensions.DependencyInjection;

namespace EvDb.UnitTests;

[EvDbAttachView<StudentStatsView>]
[EvDbAttachView<StatsView>("ALL")]
//[EvDbAttachView<MinEventIntervalSecondsView>("MinInterval")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders, EvDbSchoolOutbox>(
                        "school-records1", "students", 
                                Lifetime = ServiceLifetime.Singleton)]
public partial class SchoolStreamFactory
{
}

