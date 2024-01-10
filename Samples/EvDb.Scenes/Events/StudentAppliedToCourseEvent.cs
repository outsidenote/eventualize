using EvDb.Core;
using System.CodeDom.Compiler;

namespace EvDb.Scenes;

[EvDbEventPayload("student-applied-to-course")]
public partial record StudentAppliedToCourseEvent(int CourseId, StudentEntity Student);


[GeneratedCode("The following line should generated", "v0")]
partial record StudentAppliedToCourseEvent : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "student-applied-to-course";
}



