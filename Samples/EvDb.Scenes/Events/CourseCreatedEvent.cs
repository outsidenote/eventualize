using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefineEventPayload("course-created")]
public partial record CourseCreatedEvent(int Id, string Name, int Capacity);



