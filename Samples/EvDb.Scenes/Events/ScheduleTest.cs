using EvDb.Core;
using System.CodeDom.Compiler;

namespace EvDb.Scenes;

[EvDbEventPayload("schedule-test")]
public partial record ScheduleTest(int CourseId, TestEntity Test);




[GeneratedCode("The following line should generated", "v0")]
partial record ScheduleTest: IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "schedule-test";
}



