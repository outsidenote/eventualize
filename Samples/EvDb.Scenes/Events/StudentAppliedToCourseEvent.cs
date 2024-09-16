using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("student-applied-to-course")]
public partial record StudentAppliedToCourseEvent(int CourseId, StudentEntity Student);






