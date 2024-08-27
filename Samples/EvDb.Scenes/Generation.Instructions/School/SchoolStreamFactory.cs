using EvDb.Core;
using System.Text.Json;


namespace EvDb.UnitTests;

[EvDbAttachView<StudentStatsView>]
[EvDbAttachView<StatsView>("ALL")]
//[EvDbAttachView<MinEventIntervalSecondsView>("MinInterval")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders>("school-records", "students")]
public partial class SchoolStreamFactory
{
}