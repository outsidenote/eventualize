namespace EvDb.UnitTests;

using EvDb.Core;
using EvDb.Scenes;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static System.Formats.Asn1.AsnWriter;

public class ApiFirst
{
    [Fact]
    public async Task ApiDesign()
    {
        IEvDbAggregate<ICollection<StudentScore>, IEducationStream> agg = EvDbBuilder.Default
            .AddStreamType(new EvDbStreamType("my-domain", "education"))
            .AddEventTypes<IEducationStream>()// where T: IEvDbEventTyppes
                                              //.AddEntityId("top-users:123")
                                              //.AddAggregateType<ICollection<StudentScore>, IEducationStreamFolding>([])
            .AddTopStudentAggregateType()
            .AddEntityId("top-users:123")
            .Build();

        var course = new CourseCreated(123, "algorithm", 50);
        agg.Events.Added(course);
        //await agg.Events.CourseCreatedAsync(123, "algorithm", 50);
    }
}

public static class TopStudentFoldingExtensions
{
    public static IEvDbBuilderBuildWithEntityId<ICollection<StudentScore>, IEducationStream> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypesWithEntityId<IEducationStream> instance,
        ICollection<StudentScore> seed)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType(seed, folding);
    }

    public static IEvDbBuilderBuildWithEntityId<ICollection<StudentScore>, IEducationStream> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypesWithEntityId<IEducationStream> instance,
        Func<ICollection<StudentScore>> seedFactory)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType(seedFactory, folding);
    }

    public static IEvDbBuilderBuildWithEntityId<ICollection<StudentScore>, IEducationStream> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypesWithEntityId<IEducationStream> instance)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType([], folding);
    }

    public static IEvDbBuilderBuild<ICollection<StudentScore>, IEducationStream> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypes<IEducationStream> instance,
        ICollection<StudentScore> seed)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType(seed, folding);
    }

    public static IEvDbBuilderBuild<ICollection<StudentScore>, IEducationStream> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypes<IEducationStream> instance,
        Func<ICollection<StudentScore>> seedFactory)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType(seedFactory, folding);
    }

    public static IEvDbBuilderBuild<ICollection<StudentScore>, IEducationStream> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypes<IEducationStream> instance)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType([], folding);
    }
}

public class TopStudentFolding : IEducationStreamFolding
{
    private readonly ConcurrentDictionary<int, StudentEntity> _students = new ConcurrentDictionary<int, StudentEntity>();

    ICollection<StudentScore> IEducationStreamFolding.FoldStudentEnlisted(
        ICollection<StudentScore> state,
        StudentEnlisted enlisted)
    {
        _students.TryAdd(enlisted.Student.Id, enlisted.Student);
        return state;
    }

    ICollection<StudentScore> IEducationStreamFolding.FoldStudentReceivedGrade(
        ICollection<StudentScore> state,
        StudentReceivedGrade receivedGrade)
    {
        ICollection<StudentScore> topScores = state;
        if (!_students.TryGetValue(receivedGrade.StudentId, out StudentEntity entity))
            throw new Exception("It's broken");
        StudentScore score = new(entity, receivedGrade.Grade);
        IEnumerable<StudentScore> top = [score, .. topScores];
        ICollection<StudentScore> ordered = [.. top.OrderByDescending(x => x.Score).Take(10)];
        return ordered;
    }

    ICollection<StudentScore> IEducationStreamFolding.FoldCourseCreated(ICollection<StudentScore> state, CourseCreated courseCreated)
    {
        return state;
    }

    ICollection<StudentScore> IEducationStreamFolding.FoldScheduleTest(ICollection<StudentScore> state, ScheduleTest scheduleTest)
    {
        return state;
    }

    ICollection<StudentScore> IEducationStreamFolding.FoldStudentAppliedToCourse(ICollection<StudentScore> state, StudentAppliedToCourse applied)
    {
        return state;
    }

    ICollection<StudentScore> IEducationStreamFolding.FoldStudentCourseApplicationDenied(ICollection<StudentScore> state, StudentCourseApplicationDenied applicationDenyed)
    {
        return state;
    }

    ICollection<StudentScore> IEducationStreamFolding.FoldStudentQuitCourse(ICollection<StudentScore> state, StudentQuitCourse quit)
    {
        return state;
    }

    ICollection<StudentScore> IEducationStreamFolding.FoldStudentRegisteredToCourse(ICollection<StudentScore> state, StudentRegisteredToCourse registeredToCourse)
    {
        return state;
    }

    ICollection<StudentScore> IEducationStreamFolding.FoldStudentTestSubmitted(ICollection<StudentScore> state, StudentTestSubmitted testSubmitted)
    {
        return state;
    }
}


#region Builder Interfaces

public interface IEvDbBuilder
{
    IEvDbBuilderWithStreamType AddStreamType(EvDbStreamType streamType);
}

public interface IEvDbBuilderWithStreamType
{
    IEvDbBuilderWithEventTypesWithEntityId<TEventTypes> AddEventTypes<TEventTypes>() where TEventTypes : IEvDbEventTypes;
}

public interface IEvDbBuilderWithEventTypes<TEventTypes>
    where TEventTypes : IEvDbEventTypes
{
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState>(
        TState seed,
        IEvDbFolding<TState, TEventTypes> folding);
    // where TState : notnull, new ();
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState>(
        Func<TState> seedFactory,
        IEvDbFolding<TState, TEventTypes> folding);
    // where TState : notnull, new ();
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState>(
        IEvDbFolding<TState, TEventTypes> folding)
            where TState : notnull, new();
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState, TFolding>()
            where TState : notnull, new()
            where TFolding : IEvDbFolding<TState, TEventTypes>;
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState, TFolding>(TState seed)
            where TFolding : IEvDbFolding<TState, TEventTypes>;
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState, TFolding>(Func<TState> seedFactory)
            where TFolding : IEvDbFolding<TState, TEventTypes>;
}

public interface IEvDbBuilderWithEventTypesWithEntityId<TEventTypes> :
    IEvDbBuilderEntityId<IEvDbBuilderWithEventTypes<TEventTypes>>
    where TEventTypes : IEvDbEventTypes
{
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState>(
        TState seed,
        IEvDbFolding<TState, TEventTypes> folding);
    // where TState : notnull, new ();
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState>(
        Func<TState> seedFactory,
        IEvDbFolding<TState, TEventTypes> folding);
    // where TState : notnull, new ();
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState>(
        IEvDbFolding<TState, TEventTypes> folding)
            where TState : notnull, new();
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState, TFolding>()
            where TState : notnull, new()
            where TFolding : IEvDbFolding<TState, TEventTypes>;
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState, TFolding>(TState seed)
            where TFolding : IEvDbFolding<TState, TEventTypes>;
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState, TFolding>(Func<TState> seedFactory)
            where TFolding : IEvDbFolding<TState, TEventTypes>;
}

public interface IEvDbBuilderEntityId<T>
{
    T AddEntityId(string id);
}

public interface IEvDbBuilderBuild<TState, TEventTypes>
    // where TState: notnull, new()
    where TEventTypes : IEvDbEventTypes
{
    IEvDbAggregate<TState, TEventTypes> Build();
}

public interface IEvDbBuilderBuildWithEntityId<TState, TEventTypes> :
    IEvDbBuilderBuild<TState, TEventTypes>,
    IEvDbBuilderEntityId<IEvDbBuilderBuild<TState, TEventTypes>>
    // where TState: notnull, new()
    where TEventTypes : IEvDbEventTypes
{
}

#endregion // Builder Interfaces

public class EvDbBuilder : IEvDbBuilder
{
    public static IEvDbBuilder Default { get; } = new EvDbBuilder();

    IEvDbBuilderWithStreamType IEvDbBuilder.AddStreamType(EvDbStreamType streamType)
    {
        throw new NotImplementedException();
    }
}

public partial interface IEvDbEventTypes
{
}

public partial interface IEvDbFolding<TState, TEventTypes>
{
}

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public class EvDbEventTypeAttribute<T>: Attribute
{
    public EvDbEventTypeAttribute(string eventType)
    {
        EventType = eventType;
    }

    public string EventType { get; }
}


[EvDbEventType<CourseCreated>("course-created")]
[EvDbEventType<ScheduleTest>("schedule-test")]
[EvDbEventType<StudentAppliedToCourse>("student-applied-to-course")]
[EvDbEventType<StudentCourseApplicationDenied>("student-course-application-denied")]
[EvDbEventType<StudentEnlisted>("student-enlisted")]
[EvDbEventType<StudentQuitCourse>("student-quit-course")]
[EvDbEventType<StudentReceivedGrade>("student-received-grade")]
[EvDbEventType<StudentRegisteredToCourse>("student-registered-to-course")]
[EvDbEventType<StudentTestSubmitted>("StudentTestSubmitted")]
public partial interface IEducationStream: IEvDbEventTypes
{
    
    [GeneratedCode("The following line should generated", "v0")]
    void Added(CourseCreated courseCreated);
    void Added(ScheduleTest scheduleTest);
    void Added(StudentAppliedToCourse applied);
    void Added(StudentCourseApplicationDenied applicationDenyed);
    void Added(StudentEnlisted enlisted);
    void Added(StudentQuitCourse quit);
    void Added(StudentReceivedGrade receivedGrade);
    void Added(StudentRegisteredToCourse registeredToCourse);
    void Added(StudentTestSubmitted testSubmitted);

}

//internal partial interface IEducationStream<
//    CourseCreated,
//    ScheduleTest,
//    StudentAppliedToCourse,
//    StudentCourseApplicationDenied,
//    StudentEnlisted,
//    StudentQuitCourse,
//    StudentReceivedGrade,
//    StudentRegisteredToCourse,
//    StudentTestSubmitted> :
//        IEvDbEventTypes
//{

//}


public interface IEducationStreamX : IEvDbEventTypes
{
    Task CourseCreatedAsync(int Id, string Name, int Capacity);

    Task ScheduleTestAsync(int CourseId, TestEntity Test);
    Task StudentAppliedToCourseAsync(int CourseId, StudentEntity Student);
    Task StudentCourseApplicationDeniedAsync(int CourseId, int StudentId);
    Task StudentEnlistedAsync(StudentEntity Student);
    Task StudentQuitCourseAsync(int CourseId, int StudentId);
    Task StudentReceivedGradeAsync(int TestId, int StudentId, double Grade, string? Comments = null);
    Task StudentRegisteredToCourseAsync(int CourseId, StudentEntity Student);
    Task AddedStudentTestSubmittedAsync(int TestId, JsonElement data);
}

[GeneratedCode("from IEducationStream<T0, T1,...>", "v0")]
public interface IEducationStreamFolding :
    IEvDbFolding<ICollection<StudentScore>, IEducationStream>
{
    ICollection<StudentScore> FoldCourseCreated(
        ICollection<StudentScore> state,
        CourseCreated courseCreated);
    ICollection<StudentScore> FoldScheduleTest(
        ICollection<StudentScore> state,
        ScheduleTest scheduleTest);
    ICollection<StudentScore> FoldStudentAppliedToCourse(
        ICollection<StudentScore> state,
        StudentAppliedToCourse applied);
    ICollection<StudentScore> FoldStudentCourseApplicationDenied(
        ICollection<StudentScore> state,
        StudentCourseApplicationDenied applicationDenyed);
    ICollection<StudentScore> FoldStudentEnlisted(
        ICollection<StudentScore> state,
        StudentEnlisted enlisted);
    ICollection<StudentScore> FoldStudentQuitCourse(
        ICollection<StudentScore> state,
        StudentQuitCourse quit);
    ICollection<StudentScore> FoldStudentReceivedGrade(
        ICollection<StudentScore> state,
        StudentReceivedGrade receivedGrade);
    ICollection<StudentScore> FoldStudentRegisteredToCourse(
        ICollection<StudentScore> state,
        StudentRegisteredToCourse registeredToCourse);
    ICollection<StudentScore> FoldStudentTestSubmitted(
        ICollection<StudentScore> state,
        StudentTestSubmitted testSubmitted);
}

[GeneratedCode("from IEducationStream<T0, T1,...>", "v0")]
public interface IEducationStreamX1 : IEvDbEventTypes
{
    //Task SendAsync<T>(T item);

    //Task CourseCreatedAsync(int Id, string Name, int Capacity);
    Task AddedAsync(CourseCreated courseCreated);

    Task AddedAsync(ScheduleTest scheduleTest);
    Task AddedAsync(StudentAppliedToCourse applied);
    Task AddedAsync(StudentCourseApplicationDenied applicationDenyed);
    Task AddedAsync(StudentEnlisted enlisted);
    Task AddedAsync(StudentQuitCourse quit);
    Task AddedAsync(StudentReceivedGrade receivedGrade);
    Task AddedAsync(StudentRegisteredToCourse registeredToCourse);
    Task AddedAsync(StudentTestSubmitted testSubmitted);
}
