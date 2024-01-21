namespace EvDb.Core.Tests;

using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Scenes;
using System.Collections.Immutable;
using System.Text.Json;
using Xunit.Abstractions;

using STATE_TYPE = System.Collections.Immutable.IImmutableDictionary<int, EvDb.UnitTests.StudentStats>;

internal static class Steps
{
    private const int TEST_ID = 6628;
    private const int STUDENT_ID = 2202;
    private const int NUM_OF_GRADES = 3;

    public static string GenerateStreamId() => $"test-stream-{Guid.NewGuid():N}";

    #region CreateFactory

    private static ISchoolXFactory CreateFactory(
        ITestOutputHelper output,
        IEvDbStorageAdapter? storageAdapter)
    {
        storageAdapter = storageAdapter;
        ServiceCollection services = new();
        services.AddSingleton(storageAdapter);
        services.AddSingleton<ISchoolXFactory, SchoolXFactory>();
        var sp = services.BuildServiceProvider();
        ISchoolXFactory factory = sp.GetRequiredService<ISchoolXFactory>();
        return factory;
    }

    #endregion // CreateFactory

    #region GivenLocalAggerate

    public static ISchoolX GivenLocalAggerate(
        ITestOutputHelper output,
        IEvDbStorageAdapter? storageAdapter,
        string? streamId = null)
    {
        streamId = streamId ?? GenerateStreamId();
        ISchoolXFactory factory = CreateFactory(output, storageAdapter);
        var aggregate = factory.Create(streamId);
        return aggregate;
    }

    #endregion // GivenLocalAggerate

    #region GivenFactoryForStoredStreamWithEvents

    public static (ISchoolXFactory Factory, string StreamId) GivenFactoryForStoredStreamWithEvents(
        ITestOutputHelper output,
        IEvDbStorageAdapter storageAdapter,
        string? streamId = null,
        Action<IEvDbStorageAdapter, string>? mockGetAsyncResult = null,
        bool withEvents = true)
    {
        throw new NotImplementedException();
        streamId = streamId ?? GenerateStreamId();
        ISchoolXFactory factory = CreateFactory(output, storageAdapter);

        if (mockGetAsyncResult == null)
        {
            factory.SetupMockGetAsync(storageAdapter, streamId, withEvents: withEvents);
        }
        else
        {
            mockGetAsyncResult.Invoke(storageAdapter, streamId);
        }
        return (factory, streamId);
    }

    #endregion // GivenFactoryForStoredStreamWithEvents

    #region WhenGetAggregateAsync

    public static async Task<ISchoolX> WhenGetAggregateAsync(this (ISchoolXFactory Factory, string StreamId) input)
    {
        var (factory, streamId) = input;
        var result = await factory.GetAsync(streamId);
        return result;
    }

    #endregion // WhenGetAggregateAsync

    #region WhenAddingPendingEvents

    public static ISchoolX WhenAddingPendingEvents(this ISchoolX aggregate)
    {
        aggregate.EnlistStudent()
                  .WhenAddGrades();
        return aggregate;

    }

    #endregion // WhenAddingPendingEvents

    #region SetupMockGetAsync

    public static ISchoolXFactory SetupMockGetAsync(
        this ISchoolXFactory factory,
        IEvDbStorageAdapter storageAdapter,
        string streamId,
        JsonSerializerOptions? serializerOptions = null,
        bool withEvents = true)
    {

        A.CallTo(() => storageAdapter.GetAsync(A<EvDbStreamCursor>.Ignored, A<CancellationToken>.Ignored))
        .ReturnsLazily((EvDbStreamCursor cursor, CancellationToken ct) =>
        {
            if (withEvents)
            {
                List<EvDbStoredEvent> storedEvents = CreateStoredEvents(
                                factory,
                                streamId,
                                serializerOptions,
                                cursor.Offset);
                return storedEvents.ToAsync();
            }
            return Array.Empty<EvDbStoredEvent>().ToAsync();
        });

        return factory;
    }

    #endregion // SetupMockGetAsync

    #region SetupMockSaveThrowOcc

    public static ISchoolXFactory SetupMockSaveThrowOcc(
        this ISchoolXFactory factory,
        IEvDbStorageAdapter storageAdapter)
    {
        A.CallTo(() => storageAdapter.SaveAsync<STATE_TYPE>(A<IEvDbAggregate<STATE_TYPE>>.Ignored, A<bool>.Ignored, A<JsonSerializerOptions>.Ignored, A<CancellationToken>.Ignored))
            .ThrowsAsync(new OCCException());

        return factory;
    }

    #endregion // SetupMockSaveThrowOcc

    #region CreateStoredEvents

    private static List<EvDbStoredEvent> CreateStoredEvents(
        ISchoolXFactory factory,
        string streamId,
        JsonSerializerOptions? serializerOptions,
        long initOffset = 0)
    {
        List<EvDbStoredEvent> storedEvents = new();
        EvDbStreamCursor cursor = new(factory.Partition, streamId, initOffset);
        StudentEnlistedEvent student = Steps.CreateStudentEnlistedEvent();
        if (initOffset == 0)
        {
            var e = EvDbEventFactory.Create(student, serializerOptions);
            var eStored = new EvDbStoredEvent(e, cursor);
            storedEvents.Add(eStored);
        }

        for (int i = 1; i <= 3; i++)
        {
            EvDbStreamCursor csr = new(factory.Partition, streamId, initOffset + i);
            double gradeValue = DefaultGradeStrategy(i);
            var grade = new StudentReceivedGradeEvent(20992, student.Student.Id, gradeValue);
            var gradeEvent = EvDbEventFactory.Create(grade, serializerOptions);
            var gradeStoreEvent = new EvDbStoredEvent(gradeEvent, csr);
            storedEvents.Add(gradeStoreEvent);
        }
        return storedEvents;
    }

    #endregion // CreateStoredEvents

    #region GivenNoSnapshot

    public static (ISchoolXFactory Factory, string StreamId) GivenNoSnapshot(
        this (ISchoolXFactory Factory, string StreamId) input,
        IEvDbStorageAdapter storageAdapter)
    {
        A.CallTo(() => storageAdapter.TryGetSnapshotAsync<STATE_TYPE>(
                    A<EvDbSnapshotId>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily<Task<EvDbStoredSnapshot<STATE_TYPE>?>>(() =>
            {
                return Task.FromResult<EvDbStoredSnapshot<STATE_TYPE>?>(null);
            });

        return input;
    }

    #endregion // GivenNoSnapshot

    #region GivenHavingSnapshot

    public static (ISchoolXFactory Factory, string StreamId) GivenHavingSnapshot(
        this (ISchoolXFactory Factory, string StreamId) input,
        IEvDbStorageAdapter storageAdapter,
        out EvDbStoredSnapshot<STATE_TYPE>? snapshot)
    {
        var (factory, streamId) = input;
        EvDbStreamAddress address = new(factory.Partition, streamId);
        var student = CreateStudentEntity();
        var stat = new StudentStats(student.Name, 70, 20);
        EvDbSnapshotCursor cursor = new(address, "Stats", 10);
        STATE_TYPE state = ImmutableDictionary<int, StudentStats>.Empty
                                                    .Add(student.Id, stat);

        EvDbStoredSnapshot<STATE_TYPE>? snp =
            new EvDbStoredSnapshot<STATE_TYPE>(state, cursor);
        snapshot = snp;

        A.CallTo(() => storageAdapter.TryGetSnapshotAsync<STATE_TYPE>(
                    A<EvDbSnapshotId>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily<Task<EvDbStoredSnapshot<STATE_TYPE>?>>(async () =>
        {
            await Task.Yield();
            return snp;
        });

        return input;
    }

    #endregion // GivenHavingSnapshot

    #region CreateStudentEntity

    public static StudentEntity CreateStudentEntity(
        int studentId = 2202,
        string studentName = "Lora")
    {
        var student = new StudentEntity(studentId, studentName);
        return student;
    }

    #endregion // CreateStudentEntity

    #region CreateStudentEnlistedEvent

    public static StudentEnlistedEvent CreateStudentEnlistedEvent(
        int studentId = 2202,
        string studentName = "Lora")
    {
        var student = CreateStudentEntity(studentId, studentName);
        var studentEnlisted = new StudentEnlistedEvent(student);
        return studentEnlisted;
    }

    #endregion // CreateStudentEnlistedEvent

    #region EnlistStudent

    public static ISchoolX EnlistStudent(this ISchoolX aggregate,
        int studentId = 2202,
        string studentName = "Lora")
    {
        var studentEnlisted = CreateStudentEnlistedEvent();
        aggregate.Add(studentEnlisted);
        return aggregate;
    }

    #endregion // EnlistStudent

    #region WhenAddGrades

    public static async Task<ISchoolX> GivenAddGradesAsync(this Task<ISchoolX> aggregateAsync,
        int testId = TEST_ID,
        int studentId = STUDENT_ID,
        int numOfGrades = NUM_OF_GRADES,
        Func<int, double>? gradeStrategy = null)
    {
        var aggregate = await aggregateAsync;
        var result = aggregate.GivenAddGrades(testId, studentId, numOfGrades, gradeStrategy);
        return result;
    }

    public static ISchoolX GivenAddGrades(this ISchoolX aggregate,
        int testId = TEST_ID,
        int studentId = STUDENT_ID,
        int numOfGrades = NUM_OF_GRADES,
        Func<int, double>? gradeStrategy = null) => 
            aggregate.WhenAddGrades(testId, studentId, numOfGrades, gradeStrategy);

    public static ISchoolX WhenAddGrades(this ISchoolX aggregate,
        int testId = TEST_ID,
        int studentId = STUDENT_ID,
        int numOfGrades = NUM_OF_GRADES,
        Func<int, double>? gradeStrategy = null)
    {
        gradeStrategy = gradeStrategy ?? DefaultGradeStrategy;
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(testId, studentId, gradeStrategy(i));
            aggregate.Add(grade);
        }

        return aggregate;
    }

    #endregion // WhenAddGrades

    public static double DefaultGradeStrategy(int i) => i * 30;

    #region GivenAggregateRetrievedFromStore

    public static async Task<ISchoolX> GivenAggregateRetrievedFromStore(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output,
        bool withEvents = true)
    {
        var aggregate = await Steps
                        .GivenFactoryForStoredStreamWithEvents(output, storageAdapter, withEvents: withEvents)
                        .GivenHavingSnapshot(storageAdapter, out var snapshot)
                        .WhenGetAggregateAsync();
        return aggregate;

    }

    #endregion // GivenAggregateRetrievedFromStore

    #region GivenLocalAggregateWithPendingEvents

    public static ISchoolX GivenLocalAggregateWithPendingEvents(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output)
    {
        return GivenLocalAggerate(output, storageAdapter)
                            .WhenAddingPendingEvents();
    }

    #endregion // GivenLocalAggregateWithPendingEvents

    #region WhenAggregateIsSaved

    public async static Task<ISchoolX> WhenAggregateIsSavedAsync(
        this ISchoolX aggregate)
    {
        await aggregate.SaveAsync();
        return aggregate;
    }

    public async static Task<ISchoolX> WhenAggregateIsSavedAsync(
        this Task<ISchoolX> aggregateAsync)
    {
        var aggregate = await aggregateAsync;
        await aggregate.SaveAsync();
        return aggregate;
    }

    #endregion // WhenAggregateIsSavedAsync

    #region GivenStoredEvents

    public static async Task<ISchoolX> GivenStoredEvents(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output)
    {
        ISchoolX aggregate = await storageAdapter.GivenLocalAggregateWithPendingEvents(output)
                     .WhenAggregateIsSavedAsync();
        return aggregate;
    }

    #endregion // GivenStoredEvents
}
