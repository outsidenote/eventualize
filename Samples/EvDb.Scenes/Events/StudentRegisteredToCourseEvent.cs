using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("student-registered-to-course")]
public partial record StudentRegisteredToCourseEvent(int CourseId, StudentEntity Student);



