using EvDb.Scenes;
using System.Text.Json.Serialization;


namespace EvDb.UnitTests;

[JsonSerializable(typeof(CourseCreatedEvent))]
[JsonSerializable(typeof(ScheduleTestEvent))]
[JsonSerializable(typeof(StudentAppliedToCourseEvent))]
[JsonSerializable(typeof(StudentCourseApplicationDeniedEvent))]
[JsonSerializable(typeof(StudentEnlistedEvent))]
[JsonSerializable(typeof(StudentQuitCourseEvent))]
[JsonSerializable(typeof(StudentReceivedGradeEvent))]
[JsonSerializable(typeof(StudentRegisteredToCourseEvent))]
[JsonSerializable(typeof(StudentTestSubmittedEvent))]
//[JsonSerializable(typeof(StudentStats[]))]
//[JsonSerializable(typeof(MyStats))]
//[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.Always)]
public partial class SchoolStreamAddersContext : JsonSerializerContext
{
}
