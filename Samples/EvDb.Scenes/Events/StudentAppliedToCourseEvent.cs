using EvDb.Core;
using System.CodeDom.Compiler;

namespace EvDb.Scenes;

[EvDbEventPayload("student-applied-to-course")]
public partial record StudentAppliedToCourseEvent(int CourseId, StudentEntity Student);






