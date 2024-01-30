namespace EvDb.Core.Tests;

using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Scenes;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using Xunit.Abstractions;

using STATE_TYPE = EvDb.Scenes.StudentStatsState;

internal static class Steps
{
    private const int TEST_ID = 6628;
    private const int STUDENT_ID = 2202;
    private const int NUM_OF_GRADES = 3;

    public static string GenerateStreamId() => $"test-stream-{Guid.NewGuid():N}";
    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ?? throw new NotSupportedException("GetExecutingAssembly");
    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";

    #region CreateEvent

    private static IEvDbEvent CreateEvent<T>(
        this T data,
        IEvDbStreamStore stream,
        long offset = 0,
        string? capturedBy = null,
        JsonSerializerOptions? options = null)
        where T : IEvDbEventPayload
    {
        EvDbStreamCursor cursor = new EvDbStreamCursor(stream.StreamAddress, offset);
        var result = CreateEvent(data, cursor, capturedBy, options);
        return result;
    }

    private static IEvDbEvent CreateEvent<T>(
        this T data,
        IEvDbSchoolStreamFactory factory,
        string streamId,
        long offset = 0,
        string? capturedBy = null,
        JsonSerializerOptions? options = null)
        where T : IEvDbEventPayload
    {
        var srmId = new EvDbStreamAddress(factory.PartitionAddress, streamId);
        EvDbStreamCursor cursor = new EvDbStreamCursor(srmId, offset);
        var result = CreateEvent(data, cursor, capturedBy, options);
        return result;
    }

    private static IEvDbEvent CreateEvent<T>(
        this T data,
        EvDbStreamCursor streamCursor,
        string? capturedBy = null,
        JsonSerializerOptions? options = null)
        where T : IEvDbEventPayload
    {
        capturedBy = capturedBy ?? DEFAULT_CAPTURE_BY;
        var json = JsonSerializer.Serialize(data, options);
        var result = new EvDbEvent(data.EventType, DateTime.UtcNow, capturedBy, streamCursor, json);
        return result;
    }

    #endregion // CreateEvent

    #region CreateFactory

    private static IEvDbSchoolStreamFactory CreateFactory(
        ITestOutputHelper output,
        IEvDbStorageAdapter? storageAdapter)
    {
        storageAdapter = storageAdapter;
        ServiceCollection services = new();
        services.AddSingleton(storageAdapter);
        services.AddSingleton<IEvDbSchoolStreamFactory, SchoolStreamFactory>();
        var sp = services.BuildServiceProvider();
        IEvDbSchoolStreamFactory factory = sp.GetRequiredService<IEvDbSchoolStreamFactory>();
        return factory;
    }

    #endregion // CreateFactory

    #region GivenLocalAggerate

    public static IEvDbSchoolStream GivenLocalAggerate(
        ITestOutputHelper output,
        IEvDbStorageAdapter? storageAdapter,
        string? streamId = null)
    {
        streamId = streamId ?? GenerateStreamId();
        IEvDbSchoolStreamFactory factory = CreateFactory(output, storageAdapter);
        var aggregate = factory.Create(streamId);
        return aggregate;
    }

    #endregion // GivenLocalAggerate

    #region GivenFactoryForStoredStreamWithEvents

    public static (IEvDbSchoolStreamFactory Factory, string StreamId) GivenFactoryForStoredStreamWithEvents(
        ITestOutputHelper output,
        IEvDbStorageAdapter storageAdapter,
        string? streamId = null,
        Action<IEvDbStorageAdapter, string>? mockGetAsyncResult = null,
        bool withEvents = true)
    {
        streamId = streamId ?? GenerateStreamId();
        IEvDbSchoolStreamFactory factory = CreateFactory(output, storageAdapter);

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

    public static async Task<IEvDbSchoolStream> WhenGetAggregateAsync(this (IEvDbSchoolStreamFactory Factory, string StreamId) input)
    {
        var (factory, streamId) = input;
        var result = await factory.GetAsync(streamId);
        return result;
    }

    #endregion // WhenGetAggregateAsync

    #region WhenAddingPendingEvents

    public static IEvDbSchoolStream WhenAddingPendingEvents(this IEvDbSchoolStream aggregate)
    {
        aggregate.EnlistStudent()
                  .WhenAddGrades();
        return aggregate;

    }

    #endregion // WhenAddingPendingEvents

    #region SetupMockGetAsync

    public static IEvDbSchoolStreamFactory SetupMockGetAsync(
        this IEvDbSchoolStreamFactory factory,
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

    public static IEvDbSchoolStreamFactory SetupMockSaveThrowOcc(
        this IEvDbSchoolStreamFactory factory,
        IEvDbStorageAdapter storageAdapter)
    {
        throw new NotImplementedException() ;
        //A.CallTo(() => storageAdapter.SaveAsync<STATE_TYPE>((Core.IEvDbAggregateDeprecated<STATE_TYPE>)A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, A<bool>.Ignored, A<Options>.Ignored, A<CancellationToken>.Ignored))
        //    .ThrowsAsync(new OCCException());

        //return factory;
    }

    #endregion // SetupMockSaveThrowOcc

    #region CreateStoredEvents

    private static List<EvDbStoredEvent> CreateStoredEvents(
        IEvDbSchoolStreamFactory factory,
        string streamId,
        JsonSerializerOptions? serializerOptions,
        long initOffset = 0)
    {
        List<EvDbStoredEvent> storedEvents = new();
        StudentEnlistedEvent student = Steps.CreateStudentEnlistedEvent();
        bool withEnlisted = initOffset == 0;
        if (withEnlisted)
        {
            EvDbStreamCursor cursor = new(factory.PartitionAddress, streamId, initOffset);
            var e = student.CreateEvent(factory,streamId, options: serializerOptions);
            var eStored = new EvDbStoredEvent(e, cursor);
            storedEvents.Add(eStored);
            initOffset++;
        }
        for (int i = 0; i < 3; i++)
        {
            EvDbStreamCursor cursor = new(factory.PartitionAddress, streamId, initOffset + i);
            double gradeValue = DefaultGradeStrategy(i + 1);
            var grade = new StudentReceivedGradeEvent(20992, student.Student.Id, gradeValue);
            var gradeEvent = grade.CreateEvent(factory,streamId, options: serializerOptions);
            var gradeStoreEvent = new EvDbStoredEvent(gradeEvent, cursor);
            storedEvents.Add(gradeStoreEvent);
        }
        return storedEvents;
    }

    #endregion // CreateStoredEvents

    #region GivenNoSnapshot

    public static (IEvDbSchoolStreamFactory Factory, string StreamId) GivenNoSnapshot(
        this (IEvDbSchoolStreamFactory Factory, string StreamId) input,
        IEvDbStorageAdapter storageAdapter)
    {
        A.CallTo(() => storageAdapter.TryGetSnapshotAsync(
                    A<EvDbViewAddress>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily<Task<EvDbStoredSnapshot>>(() =>
            {
                return Task.FromResult(EvDbStoredSnapshot.Empty);
            });

        return input;
    }

    #endregion // GivenNoSnapshot

    #region GivenHavingSnapshotsWithSameOffset

    public static (IEvDbSchoolStreamFactory Factory, string StreamId) GivenHavingSnapshotsWithSameOffset(
        this (IEvDbSchoolStreamFactory Factory, string StreamId) input,
        IEvDbStorageAdapter storageAdapter)
    {
        return GivenHavingSnapshots(input, storageAdapter, _ => 60);
    }

    #endregion // GivenHavingSnapshotsWithSameOffset

    #region GivenHavingSnapshotsWithDifferentOffset

    public static (IEvDbSchoolStreamFactory Factory, string StreamId) GivenHavingSnapshotsWithDifferentOffset(
        this (IEvDbSchoolStreamFactory Factory, string StreamId) input,
        IEvDbStorageAdapter storageAdapter)
    {
        return GivenHavingSnapshots(input, storageAdapter, 
            n => n switch
            { 
                StatsView.ViewName => 60,
                StudentStatsView.ViewName => 61
            });
    }

    #endregion // GivenHavingSnapshotsWithDifferentOffset

    #region GivenHavingSnapshot

    public static (IEvDbSchoolStreamFactory Factory, string StreamId) GivenHavingSnapshots(
        this (IEvDbSchoolStreamFactory Factory, string StreamId) input,
        IEvDbStorageAdapter storageAdapter,
        Func<string, long> getSnapshotOffset)
    {
        A.CallTo(() => storageAdapter.TryGetSnapshotAsync(
                    A<EvDbViewAddress>.That.Matches(a => a.ViewName == StudentStatsView.ViewName), A<CancellationToken>.Ignored))
            .ReturnsLazily<EvDbStoredSnapshot>(() =>
                {
                    long offset = getSnapshotOffset(StudentStatsView.ViewName);
                    var snapshot = CreateStudentStatsSnapshot(offset, input.Factory.Options);
                    return snapshot;
                });

        A.CallTo(() => storageAdapter.TryGetSnapshotAsync(
                    A<EvDbViewAddress>.That.Matches(a => a.ViewName == StatsView.ViewName), A<CancellationToken>.Ignored))
            .ReturnsLazily<EvDbStoredSnapshot>(() =>
                {
                    long offset = getSnapshotOffset(StatsView.ViewName);
                    var snapshot = CreateStatsSnapshot(offset, input.Factory.Options);
                    return snapshot;
                });

        return input;
    }

    #endregion // GivenHavingSnapshot

    #region CreateStudentStatsSnapshot

    public static EvDbStoredSnapshot CreateStudentStatsSnapshot(long offset, JsonSerializerOptions? options)
    {
        var student = CreateStudentEntity();
        var stat = new StudentStats(student.Id, student.Name, 70, 20);

        STATE_TYPE state = new STATE_TYPE() { Students = [stat] };
        string json = JsonSerializer.Serialize(state, options);
        EvDbStoredSnapshot snp =
            new EvDbStoredSnapshot(offset, json);
        return snp;
    }

    #endregion // CreateStudentStatsSnapshot

    #region CreateStatsSnapshot

    public static EvDbStoredSnapshot CreateStatsSnapshot(long offset, JsonSerializerOptions? options)
    {
        var state = new Stats(200, 100);

        string json = JsonSerializer.Serialize(state, options);
        EvDbStoredSnapshot snp =
            new EvDbStoredSnapshot(offset, json);
        return snp;
    }

    #endregion // CreateStatsSnapshot

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

    public static IEvDbSchoolStream EnlistStudent(this IEvDbSchoolStream aggregate,
        int studentId = 2202,
        string studentName = "Lora")
    {
        var studentEnlisted = CreateStudentEnlistedEvent();
        aggregate.Add(studentEnlisted);
        return aggregate;
    }

    #endregion // EnlistStudent

    #region WhenAddGrades

    public static async Task<IEvDbSchoolStream> GivenAddGradesAsync(this Task<IEvDbSchoolStream> aggregateAsync,
        int testId = TEST_ID,
        int studentId = STUDENT_ID,
        int numOfGrades = NUM_OF_GRADES,
        Func<int, double>? gradeStrategy = null)
    {
        var aggregate = await aggregateAsync;
        var result = aggregate.GivenAddGrades(testId, studentId, numOfGrades, gradeStrategy);
        return result;
    }

    public static IEvDbSchoolStream GivenAddGrades(this IEvDbSchoolStream aggregate,
        int testId = TEST_ID,
        int studentId = STUDENT_ID,
        int numOfGrades = NUM_OF_GRADES,
        Func<int, double>? gradeStrategy = null) => 
            aggregate.WhenAddGrades(testId, studentId, numOfGrades, gradeStrategy);

    public static IEvDbSchoolStream WhenAddGrades(this IEvDbSchoolStream aggregate,
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

    #region GivenStreamRetrievedFromStore

    public static async Task<IEvDbSchoolStream> GivenStreamRetrievedFromStore(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output,
        bool withEvents = true)
    {
        var stream = await Steps
                        .GivenFactoryForStoredStreamWithEvents(output, storageAdapter, withEvents: withEvents)
                        .GivenHavingSnapshotsWithSameOffset(storageAdapter)
                        .WhenGetAggregateAsync();
        return stream;

    }

    #endregion // GivenStreamRetrievedFromStore

    #region GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset

    public static async Task<IEvDbSchoolStream> GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output,
        bool withEvents = true)
    {
        var stream = await Steps
                        .GivenFactoryForStoredStreamWithEvents(output, storageAdapter, withEvents: withEvents)
                        .GivenHavingSnapshotsWithDifferentOffset(storageAdapter)
                        .WhenGetAggregateAsync();
        return stream;

    }

    #endregion // GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset

    #region GivenLocalStreamWithPendingEvents

    public static IEvDbSchoolStream GivenLocalStreamWithPendingEvents(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output)
    {
        return GivenLocalAggerate(output, storageAdapter)
                            .WhenAddingPendingEvents();
    }

    #endregion // GivenLocalStreamWithPendingEvents

    #region WhenAggregateIsSaved

    public async static Task<IEvDbSchoolStream> WhenAggregateIsSavedAsync(
        this IEvDbSchoolStream aggregate)
    {
        await aggregate.SaveAsync();
        return aggregate;
    }

    public async static Task<IEvDbSchoolStream> WhenAggregateIsSavedAsync(
        this Task<IEvDbSchoolStream> aggregateAsync)
    {
        var aggregate = await aggregateAsync;
        await aggregate.SaveAsync();
        return aggregate;
    }

    #endregion // WhenAggregateIsSavedAsync

    #region GivenStoredEvents

    public static async Task<IEvDbSchoolStream> GivenStoredEvents(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output)
    {
        IEvDbSchoolStream aggregate = await storageAdapter.GivenLocalStreamWithPendingEvents(output)
                     .WhenAggregateIsSavedAsync();
        return aggregate;
    }

    #endregion // GivenStoredEvents
}
