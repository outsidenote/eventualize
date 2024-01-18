using EvDb.Core;

namespace EvDb.Scenes;

[EvDbEventPayload("student-applied-to-course")]
public partial record StudentAppliedToCourseEvent(int CourseId, StudentEntity Student);






