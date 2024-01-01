using System.Collections.Immutable;

namespace EvDb.Scenes;

public record CourseAttendees(CourseEntity Course, IImmutableList<StudentEntity> Students);



