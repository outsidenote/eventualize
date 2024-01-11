using EvDb.Core;
using System.CodeDom.Compiler;

namespace EvDb.Scenes;

[EvDbEventPayload("schedule-test")]
public partial record ScheduleTestEvent(int CourseId, TestEntity Test);




