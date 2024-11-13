using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefineEventPayload("student-applied-to-course")]
public partial record StudentAppliedToCourseEvent(int CourseId, StudentEntity Student);






