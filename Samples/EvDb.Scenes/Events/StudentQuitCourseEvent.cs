using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("student-quit-course")]
public partial record StudentQuitCourseEvent(int CourseId, int StudentId);





