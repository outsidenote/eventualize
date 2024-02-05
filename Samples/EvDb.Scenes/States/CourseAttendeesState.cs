using System.Collections.Immutable;

namespace EvDb.Scenes;

public record CourseAttendeesState(CourseEntity Course, IImmutableList<StudentEntity> Students);



