using EvDb.Core;

namespace EvDb.Scenes;

[EvDbEventPayload("student-quit-course")]
public partial record StudentQuitCourseEvent(int CourseId, int StudentId);





