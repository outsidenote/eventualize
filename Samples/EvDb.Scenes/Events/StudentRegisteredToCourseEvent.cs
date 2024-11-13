using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefineEventPayload("student-registered-to-course")]
public partial record StudentRegisteredToCourseEvent(int CourseId, StudentEntity Student);



