using EvDb.Scenes;
using EvDb.UnitTests;

[assembly: EvDbAggregateType<ICollection<StudentScore>, IEducationEventTypes>(
    "top-student")]

namespace EvDb.UnitTests;

using EvDb.Core;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;

public class ApiFirst
{
    [Fact]
    public async Task ApiDesign()
    {
        //var agg = EvDbBuilder.Default
        //    .AddStreamType(new EvDbStreamType("my-domain", "education"))
        //    .AddEventTypes<IEducationStream>()
        //    // .AddAggregateType<ICollection<StudentScore>, IEducationStreamFolding>([])
        //    .AddTopStudentAggregateType([]);

        TopStudentFolding folding = new ();
        IEvDbAggregate<ICollection<StudentScore>, IEducationEventTypes> agg = EvDbBuilder.Default
            .AddPartition<IEducationEventTypes>("my-domain", "education")
            //.AddPartition("my-domain", "education")
            //.AddStreamType<IEducationEventTypes>()// where T: IEvDbEventTyppes
            //.AddEntityId("top-users:123")
            //.AddAggregateType<ICollection<StudentScore>, IEducationStreamFolding>([])
            //.AddAggregateType<TopStudentFolding>(isDev ? 4 : 100)
            .AddAggregateType(folding.Default, folding)
            .AddStreamId("top-users:123")
            //.AddTopStudentAggregateType()
            //.AddTopStudentAggregateType<TopStudentFolding>()
            .Build();

        var course = new CourseCreated(123, "algorithm", 50);
        agg.Events.Add(course);
        //await agg.Events.CourseCreatedAsync(123, "algorithm", 50);
    }
}

#region Builder Interfaces

public interface IEvDbBuilder
{

    IEvDbBuilderWithEventTypesWithEntityId<TEventTypes> AddPartition<TEventTypes>(string domain, string partition)
            where TEventTypes : IEvDbEventTypes;
}


public interface IEvDbBuilderWithEventTypes<TEventTypes>
    where TEventTypes : IEvDbEventTypes
{
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState>(
        TState seed,
        IEvDbAggregateType<TState, TEventTypes> folding);
    // where TState : notnull, new ();
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState>(
        Func<TState> seedFactory,
        IEvDbAggregateType<TState, TEventTypes> folding);
    // where TState : notnull, new ();
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState>(
        IEvDbAggregateType<TState, TEventTypes> folding)
            where TState : notnull, new();
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState, TFolding>()
            where TState : notnull, new()
            where TFolding : IEvDbAggregateType<TState, TEventTypes>;
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState, TFolding>(TState seed)
            where TFolding : IEvDbAggregateType<TState, TEventTypes>;
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState, TFolding>(Func<TState> seedFactory)
            where TFolding : IEvDbAggregateType<TState, TEventTypes>;
}

public interface IEvDbBuilderWithEventTypesWithEntityId<TEventTypes> :
    IEvDbBuilderEntityId<IEvDbBuilderWithEventTypes<TEventTypes>>
    where TEventTypes : IEvDbEventTypes
{
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState>(
        TState seed,
        IEvDbAggregateType<TState, TEventTypes> folding);
    // where TState : notnull, new ();
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState>(
        Func<TState> seedFactory,
        IEvDbAggregateType<TState, TEventTypes> folding);
    // where TState : notnull, new ();
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState>(
        IEvDbAggregateType<TState, TEventTypes> folding)
            where TState : notnull, new();
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState, TFolding>()
            where TState : notnull, new()
            where TFolding : IEvDbAggregateType<TState, TEventTypes>;
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState, TFolding>(TState seed)
            where TFolding : IEvDbAggregateType<TState, TEventTypes>;
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState, TFolding>(Func<TState> seedFactory)
            where TFolding : IEvDbAggregateType<TState, TEventTypes>;
}

public interface IEvDbBuilderEntityId<T>
{
    T AddStreamId(string id);
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

//[GenerateBuilderPattern]
internal partial record EvDbSetupState
{
    public EvDbPartition? Partition { get; init; }
}

public class EvDbBuilder : IEvDbBuilder,
                           IEvDbBuilderWithStreamType
{
    protected readonly EvDbPartition? _streamType;

    protected EvDbBuilder(EvDbPartition? streamType = null)
    {
        _streamType = streamType;
    }   
    
    public static IEvDbBuilder Default { get; } = new EvDbBuilder();

    IEvDbBuilderWithStreamType IEvDbBuilder.AddPartition<TEventTypes>(string domain, string partition)
    {
        EvDbPartition streamType = new(domain, partition);
        return new EvDbBuilder<TEventTypes>(streamType);
    }

    IEvDbBuilderWithEventTypesWithEntityId<TEventTypes> IEvDbBuilderWithStreamType.AddStreamType<TEventTypes>()
    {
        return new EvDbBuilder<TEventTypes>(_streamType!);
    }
}

internal class EvDbBuilder<TEventTypes> :
                            EvDbBuilder,
                            IEvDbBuilderWithEventTypesWithEntityId<TEventTypes>
    where TEventTypes : IEvDbEventTypes
{
    private readonly EvDbPartition _streamType;

    public EvDbBuilder(EvDbPartition streamType): base(streamType)
    {
        _streamType = streamType;
    }

    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> IEvDbBuilderWithEventTypesWithEntityId<TEventTypes>.AddAggregateType<TState>(TState seed, IEvDbAggregateType<TState, TEventTypes> folding)
    {
        throw new NotImplementedException();
    }

    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> IEvDbBuilderWithEventTypesWithEntityId<TEventTypes>.AddAggregateType<TState>(Func<TState> seedFactory, IEvDbAggregateType<TState, TEventTypes> folding)
    {
        throw new NotImplementedException();
    }

    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> IEvDbBuilderWithEventTypesWithEntityId<TEventTypes>.AddAggregateType<TState>(IEvDbAggregateType<TState, TEventTypes> folding)
    {
        throw new NotImplementedException();
    }

    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> IEvDbBuilderWithEventTypesWithEntityId<TEventTypes>.AddAggregateType<TState, TFolding>()
    {
        throw new NotImplementedException();
    }

    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> IEvDbBuilderWithEventTypesWithEntityId<TEventTypes>.AddAggregateType<TState, TFolding>(TState seed)
    {
        throw new NotImplementedException();
    }

    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> IEvDbBuilderWithEventTypesWithEntityId<TEventTypes>.AddAggregateType<TState, TFolding>(Func<TState> seedFactory)
    {
        throw new NotImplementedException();
    }

    IEvDbBuilderWithEventTypes<TEventTypes> IEvDbBuilderEntityId<IEvDbBuilderWithEventTypes<TEventTypes>>.AddStreamId(string id)
    {
        throw new NotImplementedException();
    }
}
internal class EvDbBuilder<TEventTypes, TState> :
                            EvDbBuilder,
                            IEvDbBuilderBuildWithEntityId<TState, TEventTypes>
    where TEventTypes : IEvDbEventTypes
{
    private readonly EvDbPartition _streamType;
    private readonly TState _seed;

    public EvDbBuilder(EvDbPartition streamType, TState seed) : base(streamType)
    {
        _streamType = streamType;
        _seed = seed;
    }

    IEvDbBuilderBuild<TState, TEventTypes> IEvDbBuilderEntityId<IEvDbBuilderBuild<TState, TEventTypes>>.AddStreamId(string id)
    {
        throw new NotImplementedException();
    }

    IEvDbAggregate<TState, TEventTypes> IEvDbBuilderBuild<TState, TEventTypes>.Build()
    {
        throw new NotImplementedException();
    }
}

#region Core

public interface IEvDbEventTypes { }

public interface IEvDbAggregateType<TState, TEventTypes> 
{
    /// <summary>
    /// Represents the seed state of the aggregation (before folding).
    /// </summary>
    TState Default { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    string Name { get; }
}

#endregion // Core

#region [EvDbEventType]

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public class EvDbEventTypeAttribute<T>: Attribute
{
    public EvDbEventTypeAttribute(string eventType)
    {
        EventType = eventType;
    }

    public string EventType { get; }
}

#endregion // [EvDbEventType]

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Assembly, AllowMultiple = true)]
public class EvDbAggregateTypeAttribute<TState, TEventType>: Attribute
{
    public EvDbAggregateTypeAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

#region IEducationStream (generation)

[EvDbEventType<CourseCreated>("course-created")]
[EvDbEventType<ScheduleTest>("schedule-test")]
[EvDbEventType<StudentAppliedToCourse>("student-applied-to-course")]
[EvDbEventType<StudentCourseApplicationDenied>("student-course-application-denied")]
[EvDbEventType<StudentEnlisted>("student-enlisted")]
[EvDbEventType<StudentQuitCourse>("student-quit-course")]
[EvDbEventType<StudentReceivedGrade>("student-received-grade")]
[EvDbEventType<StudentRegisteredToCourse>("student-registered-to-course")]
[EvDbEventType<StudentTestSubmitted>("StudentTestSubmitted")]
public partial interface IEducationEventTypes:
    IEvDbEventTypes // TODO: generate this one
{
    [GeneratedCode("The following line should generated", "v0")]
    void Add(CourseCreated courseCreated);
    void Add(ScheduleTest scheduleTest);
    void Add(StudentAppliedToCourse applied);
    void Add(StudentCourseApplicationDenied applicationDenyed);
    void Add(StudentEnlisted enlisted);
    void Add(StudentQuitCourse quit);
    void Add(StudentReceivedGrade receivedGrade);
    void Add(StudentRegisteredToCourse registeredToCourse);
    void Add(StudentTestSubmitted testSubmitted);
}

#endregion // IEducationStream (generation)

#region ITopStudentFolding (generation)

//[EvDbAggregateType<ICollection<StudentScore>, IEducationEventTypes>("top-student")]
//public interface ITopStudentAggregateType :
//    //[GeneratedCode("from IEducationStream<T0, T1,...>", "v0")]
//    IEvDbAggregateType<ICollection<StudentScore>, IEducationEventTypes>
//{ 
//}

[GeneratedCode("from IEducationEventTypes<T0, T1,...>", "v0")]
public abstract partial class  TopStudentAggregateTypeBase : IEvDbAggregateType<ICollection<StudentScore>, IEducationEventTypes>
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

#endregion // ITopStudentFolding (generation)

#region TopStudentFolding (user code)

public class TopStudentFolding : TopStudentAggregateTypeBase
{
    //public TopStudentFolding(): base(this)
    //{

    //}

    private readonly ConcurrentDictionary<int, StudentEntity> _students = new ConcurrentDictionary<int, StudentEntity>();

    public override ICollection<StudentScore> Default { get; } = [];

    public override ICollection<StudentScore> Fold(
        ICollection<StudentScore> state,
        StudentEnlisted enlisted)
    {
        _students.TryAdd(enlisted.Student.Id, enlisted.Student);
        return state;
    }

    public override ICollection<StudentScore> Fold(
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
}

#endregion // TopStudentFolding (user code)

#region Extension Methods (generated)

// generated when identify implementation of IEvDbFolding<ICollection<StudentScore>, IEducationStream>

public static class TopStudentFoldingExtensions
{
    public static IEvDbBuilderBuildWithEntityId<ICollection<StudentScore>, IEducationEventTypes> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypesWithEntityId<IEducationEventTypes> instance,
        ICollection<StudentScore> seed)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType(seed, folding);
    }

    public static IEvDbBuilderBuildWithEntityId<ICollection<StudentScore>, IEducationEventTypes> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypesWithEntityId<IEducationEventTypes> instance,
        Func<ICollection<StudentScore>> seedFactory)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType(seedFactory, folding);
    }

    public static IEvDbBuilderBuildWithEntityId<ICollection<StudentScore>, IEducationEventTypes> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypesWithEntityId<IEducationEventTypes> instance)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType([], folding);
    }

    public static IEvDbBuilderBuild<ICollection<StudentScore>, IEducationEventTypes> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypes<IEducationEventTypes> instance,
        ICollection<StudentScore> seed)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType(seed, folding);
    }

    public static IEvDbBuilderBuild<ICollection<StudentScore>, IEducationEventTypes> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypes<IEducationEventTypes> instance,
        Func<ICollection<StudentScore>> seedFactory)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType(seedFactory, folding);
    }

    public static IEvDbBuilderBuild<ICollection<StudentScore>, IEducationEventTypes> AddTopStudentAggregateType(
        this IEvDbBuilderWithEventTypes<IEducationEventTypes> instance)
    {
        TopStudentFolding folding = new();
        return instance.AddAggregateType([], folding);
    }
}

#endregion // Extension Methods
