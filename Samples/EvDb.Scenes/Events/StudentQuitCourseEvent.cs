using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefinePayload("student-quit-course")]
public partial record StudentQuitCourseEvent(int CourseId, int StudentId);





