using EvDb.Core;
using EvDb.Scenes;
using System.CodeDom.Compiler;

namespace EvDb.UnitTests;

[GeneratedCode("from IEducationEventTypes<T0, T1,...>", "v0")]
public abstract partial class TopStudentAggregateTypeBase : IEvDbAggregateType<ICollection<StudentScore>, IEducationEventTypes>
{
    public abstract ICollection<StudentScore> Default { get; }

    public string Name { get; } = "top-student";

    public virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        CourseCreated courseCreated) => state;
    public virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        ScheduleTest scheduleTest) => state;
    public virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentAppliedToCourse applied) => state;
    public virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentCourseApplicationDenied applicationDenyed) => state;
    public virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentEnlisted enlisted) => state;
    public virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentQuitCourse quit) => state;
    public virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentReceivedGrade receivedGrade) => state;
    public virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentRegisteredToCourse registeredToCourse) => state;
    public virtual ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentTestSubmitted testSubmitted) => state;
}
