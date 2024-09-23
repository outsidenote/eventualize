using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefinePayload("course-created")]
public partial record CourseCreatedEvent(int Id, string Name, int Capacity);



