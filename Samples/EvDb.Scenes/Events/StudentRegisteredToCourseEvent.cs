using EvDb.Core;

namespace EvDb.Scenes;

[EvDbEventPayload("student-registered-to-course")]
public partial record StudentRegisteredToCourseEvent(int CourseId, StudentEntity Student);



