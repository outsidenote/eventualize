using EvDb.Core;
using System.Text.Json.Serialization;

namespace EvDb.Scenes;

[EvDbEventPayload("course-created")]
public partial record CourseCreatedEvent(int Id, string Name, int Capacity);



