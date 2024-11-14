using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefineEventPayload("schedule-test")]
public partial record ScheduleTestEvent(int CourseId, TestEntity Test);




