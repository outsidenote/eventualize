using EvDb.Core;
using EvDb.Scenes;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace EvDb.UnitTests;

[GeneratedCode("The following line should generated", "v0")]
partial interface IEducationEventTypes :
    IEvDbEventTypes 
{
    void Add(CourseCreatedEvent payload, string? capturedBy = null);
    void Add(ScheduleTestEvent payload, string? capturedBy = null);
    void Add(StudentAppliedToCourseEvent payload, string? capturedBy = null);
    void Add(StudentCourseApplicationDeniedEvent payload, string? capturedBy = null);
    void Add(StudentEnlistedEvent payload, string? capturedBy = null);
    void Add(StudentQuitCourseEvent payload, string? capturedBy = null);
    void Add(StudentReceivedGradeEvent payload, string? capturedBy = null);
    void Add(StudentRegisteredToCourseEvent payload, string? capturedBy = null);
    void Add(StudentTestSubmittedEvent payload, string? capturedBy = null);
}
