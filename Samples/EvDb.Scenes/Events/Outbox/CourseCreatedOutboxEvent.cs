// Ignore Spelling: Outbox

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("public-course-created")]
public partial record CourseCreatedOutboxEvent(int Id, string Name, int Sum);



