using EvDb.Core;
using System.Text.Json;


namespace EvDb.UnitTests;

[EvDbAttachView<StudentStatsView>]
[EvDbAttachView<StatsView>("ALL")]
//[EvDbAttachView<MinEventIntervalSecondsView>("MinInterval")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders, IEvDbSchoolStreamPublication>("school-records", "students")]
public partial class SchoolStreamFactory
{
}