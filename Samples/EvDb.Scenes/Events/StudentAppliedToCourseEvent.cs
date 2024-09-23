using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefinePayload("student-applied-to-course")]
public partial record StudentAppliedToCourseEvent(int CourseId, StudentEntity Student);






