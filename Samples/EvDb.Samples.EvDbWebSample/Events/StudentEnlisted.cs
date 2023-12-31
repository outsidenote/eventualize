// Ignore Spelling: Regidterd

using System.Collections.Immutable;
using System.Text.Json;

namespace EvDb.Samples.EvDbWebSample.Entitties;

public record StudentEntity (int Id, string Name);
public record TestEntity (int Id, string Description, DateTimeOffset Date);
public record CourseEntity (int Id, string Name);

//-------------- events -------------
public record StudentEnlisted (StudentEntity Student);
public record CourseCreated(int Id, string Name, int Capacity);
public record StudentAppliedToCourse(int CourseId, StudentEntity Student);
public record StudentCourseApplicationDenied(int CourseId, int StudentId);
public record StudentRegisteredToCourse (int CourseId, StudentEntity Student);
public record StudentQuitCourse (int CourseId, int StudentId);
public record CourseScheduleTest (int CourseId, TestEntity Test);
public record StudentSubmittedTest (int TestId, JsonElement data);
public record StudentReceivedGrade (int TestId, int StudentId, double Grade, string? Comments = null);


public enum Affinity
{
    Join,
    Leave
}

// ---- Aggregates ----

public record StudentAveregeScore (StudentEntity Student, double Score);
public record StudentAveregeScorePerCourse (
                    StudentEntity Student, string CourseName, double Score);
public record StudentInCourse(CourseEntity Course, IImmutableList<StudentEntity> Students);



