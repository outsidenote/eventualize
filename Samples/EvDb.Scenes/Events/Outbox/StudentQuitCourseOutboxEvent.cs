// Ignore Spelling: Outbox

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("public-student-quit-course")]
public partial record StudentQuitCourseOutboxEvent(int StudentId, string Name);




