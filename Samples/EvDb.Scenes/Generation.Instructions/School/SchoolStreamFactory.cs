using EvDb.Core;
using EvDb.Core.Internals;
using EvDb.Scenes;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EvDb.UnitTests;

[EvDbAttachView<StudentStatsView>]
[EvDbAttachView<StatsView>("ALL")]
//[EvDbAttachView<MinEventIntervalSecondsView>("MinInterval")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders, EvDbSchoolStreamPublicationFold>("school-records", "students")]
public partial class SchoolStreamFactory
{

}

