using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefinePayload("student-registered-to-course")]
public partial record StudentRegisteredToCourseEvent(int CourseId, StudentEntity Student);



