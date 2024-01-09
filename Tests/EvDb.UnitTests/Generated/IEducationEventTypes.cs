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
    void Add(CourseCreated payload, string? capturedBy = null);
    void Add(ScheduleTest payload, string? capturedBy = null);
    void Add(StudentAppliedToCourse payload, string? capturedBy = null);
    void Add(StudentCourseApplicationDenied payload, string? capturedBy = null);
    void Add(StudentEnlisted payload, string? capturedBy = null);
    void Add(StudentQuitCourse payload, string? capturedBy = null);
    void Add(StudentReceivedGrade payload, string? capturedBy = null);
    void Add(StudentRegisteredToCourse payload, string? capturedBy = null);
    void Add(StudentTestSubmitted payload, string? capturedBy = null);
}
