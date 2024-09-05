using EvDb.Core;

namespace EvDb.Scenes;

[EvDbEventPayload("public-student-quit-course")]
public partial record StudentQuitCoursePublicEvent(int StudentId, string Name);





