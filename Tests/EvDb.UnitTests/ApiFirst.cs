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

        await agg.Events.CourseCreatedAsync(123, "algorithm", 50);
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
        return instance.AddAggregateType([],folding);
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

    ValueTask<ICollection<StudentScore>> IEducationStreamFolding.StudentEnlistedAsync(
        ICollection<StudentScore> state,
        StudentEnlisted enlisted)
    {
        _students.TryAdd(enlisted.Student.Id, enlisted.Student);
        return ValueTask.FromResult(state);
    }

    async ValueTask<ICollection<StudentScore>> IEducationStreamFolding.StudentReceivedGradeAsync(
        ICollection<StudentScore> state,
        StudentReceivedGrade receivedGrade)
    {
        ICollection<StudentScore> topScores = state;
        if (topScores.Count < 10)
        {
            if (!_students.TryGetValue(receivedGrade.StudentId, out StudentEntity entity))
                throw new Exception("It's broken");
            StudentScore score = new(entity, receivedGrade.Grade);
            IEnumerable<StudentScore> top = [score, .. topScores];
            ICollection<StudentScore> ordered = [.. top.OrderByDescending(x => x.Score)];
            if (Interlocked.CompareExchange(ref state, ordered, topScores) != state)
            {
                IEducationStreamFolding self = this;
                var result =
                    await self.StudentReceivedGradeAsync(
                            state,
                            receivedGrade);
                return result;
            }
        }
        return state;
    }

    ValueTask<ICollection<StudentScore>> IEducationStreamFolding.CourseCreatedAsync(ICollection<StudentScore> state, CourseCreated courseCreated)
    {
        return ValueTask.FromResult(state);
    }

    ValueTask<ICollection<StudentScore>> IEducationStreamFolding.ScheduleTestAsync(ICollection<StudentScore> state, ScheduleTest scheduleTest)
    {
        return ValueTask.FromResult(state);
    }

    ValueTask<ICollection<StudentScore>> IEducationStreamFolding.StudentAppliedToCourseAsync(ICollection<StudentScore> state, StudentAppliedToCourse applied)
    {
        return ValueTask.FromResult(state);
    }

    ValueTask<ICollection<StudentScore>> IEducationStreamFolding.StudentCourseApplicationDeniedAsync(ICollection<StudentScore> state, StudentCourseApplicationDenied applicationDenyed)
    {
        return ValueTask.FromResult(state);
    }

    ValueTask<ICollection<StudentScore>> IEducationStreamFolding.StudentQuitCourseAsync(ICollection<StudentScore> state, StudentQuitCourse quit)
    {
        return ValueTask.FromResult(state);
    }

    ValueTask<ICollection<StudentScore>> IEducationStreamFolding.StudentRegisteredToCourseAsync(ICollection<StudentScore> state, StudentRegisteredToCourse registeredToCourse)
    {
        return ValueTask.FromResult(state);
    }

    ValueTask<ICollection<StudentScore>> IEducationStreamFolding.StudentTestSubmittedAsync(ICollection<StudentScore> state, StudentTestSubmitted testSubmitted)
    {
        return ValueTask.FromResult(state);
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

//[EvDbStream("Domain", "EntityType")]
internal partial interface IEducationStream<CourseCreated, ScheduleTest, StudentAppliedToCourse, StudentCourseApplicationDenied> : IEvDbEventTypes
{
}

public interface IEducationStream : IEvDbEventTypes
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
    ValueTask<ICollection<StudentScore>> CourseCreatedAsync(
        ICollection<StudentScore> state,
        CourseCreated courseCreated);
    ValueTask<ICollection<StudentScore>> ScheduleTestAsync(
        ICollection<StudentScore> state,
        ScheduleTest scheduleTest);
    ValueTask<ICollection<StudentScore>> StudentAppliedToCourseAsync(
        ICollection<StudentScore> state,
        StudentAppliedToCourse applied);
    ValueTask<ICollection<StudentScore>> StudentCourseApplicationDeniedAsync(
        ICollection<StudentScore> state,
        StudentCourseApplicationDenied applicationDenyed);
    ValueTask<ICollection<StudentScore>> StudentEnlistedAsync(
        ICollection<StudentScore> state,
        StudentEnlisted enlisted);
    ValueTask<ICollection<StudentScore>> StudentQuitCourseAsync(
        ICollection<StudentScore> state,
        StudentQuitCourse quit);
    ValueTask<ICollection<StudentScore>> StudentReceivedGradeAsync(
        ICollection<StudentScore> state,
        StudentReceivedGrade receivedGrade);
    ValueTask<ICollection<StudentScore>> StudentRegisteredToCourseAsync(
        ICollection<StudentScore> state,
        StudentRegisteredToCourse registeredToCourse);
    ValueTask<ICollection<StudentScore>> StudentTestSubmittedAsync(
        ICollection<StudentScore> state,
        StudentTestSubmitted testSubmitted);
}

[GeneratedCode("from IEducationStream<T0, T1,...>", "v0")]
public interface IEducationStreamX : IEvDbEventTypes
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
