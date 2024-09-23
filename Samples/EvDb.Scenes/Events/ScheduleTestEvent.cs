using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefinePayload("schedule-test")]
public partial record ScheduleTestEvent(int CourseId, TestEntity Test);




