using EvDb.Core;
using EvDb.Scenes;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.UnitTests;

[GeneratedCode("The following line should generated", "v0")]
partial interface IEducationEventTypes :
    IEvDbEventTypes 
{
    void Add(CourseCreated courseCreated);
    void Add(ScheduleTest scheduleTest);
    void Add(StudentAppliedToCourse applied);
    void Add(StudentCourseApplicationDenied applicationDenyed);
    void Add(StudentEnlisted enlisted);
    void Add(StudentQuitCourse quit);
    void Add(StudentReceivedGrade receivedGrade);
    void Add(StudentRegisteredToCourse registeredToCourse);
    void Add(StudentTestSubmitted testSubmitted);
}
