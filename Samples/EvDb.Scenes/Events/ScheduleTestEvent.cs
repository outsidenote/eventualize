using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("schedule-test")]
public partial record ScheduleTestEvent(int CourseId, TestEntity Test);




