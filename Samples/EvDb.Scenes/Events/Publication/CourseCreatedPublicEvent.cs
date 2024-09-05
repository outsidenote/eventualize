using EvDb.Core;

namespace EvDb.Scenes;

[EvDbEventPayload("public-course-created")]
public partial record CourseCreatedPublicEvent(int Id, string Name, int Sum);



