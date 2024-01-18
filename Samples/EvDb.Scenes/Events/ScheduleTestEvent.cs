using EvDb.Core;

namespace EvDb.Scenes;

[EvDbEventPayload("schedule-test")]
public partial record ScheduleTestEvent(int CourseId, TestEntity Test);




