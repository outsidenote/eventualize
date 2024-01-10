using EvDb.Core;
using System.CodeDom.Compiler;

namespace EvDb.Scenes;

[EvDbEventPayload("schedule-test")]
public partial record ScheduleTestEvent(int CourseId, TestEntity Test);




[GeneratedCode("The following line should generated", "v0")]
partial record ScheduleTestEvent: IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "schedule-test";
}



