using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("course-created")]
public partial record CourseCreatedEvent(int Id, string Name, int Capacity);



