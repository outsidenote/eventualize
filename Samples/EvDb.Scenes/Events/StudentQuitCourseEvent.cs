using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefineEventPayload("student-quit-course")]
public partial record StudentQuitCourseEvent(int CourseId, int StudentId);





