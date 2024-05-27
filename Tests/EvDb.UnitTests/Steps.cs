namespace EvDb.Core.Tests;

using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Scenes;
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

    private static EvDbEvent CreateEvent<T>(
        this T data,
        EvDbStreamCursor streamCursor,
        string? capturedBy = null,
        JsonSerializerOptions? options = null)
        where T : IEvDbEventPayload
    {
        capturedBy = capturedBy ?? DEFAULT_CAPTURE_BY;
        var json = JsonSerializer.Serialize(data, options);
        var result = new EvDbEvent(data.EventType, DateTimeOffset.UtcNow, capturedBy, streamCursor, json);
        return result;
    }

    #endregion // CreateEvent

    #region CreateFactory

    private static IEvDbSchoolStreamFactory CreateFactory(
                IEvDbStorageAdapter storageAdapter,
                TimeProvider? timeProvider)
    {
        ServiceCollection services = new();
        services.AddSingleton(storageAdapter);
        services.AddSingleton<IEvDbSchoolStreamFactory, SchoolStreamFactory>();
        services.AddSingleton<TimeProvider>(timeProvider ?? TimeProvider.System);
        var sp = services.BuildServiceProvider();
        IEvDbSchoolStreamFactory factory = sp.GetRequiredService<IEvDbSchoolStreamFactory>();
        return factory;
    }

    #endregion // CreateFactory

    #region GivenLocalAggerate

    public static IEvDbSchoolStream GivenLocalStream(
        this IEvDbStorageAdapter storageAdapter,
        string? streamId = null,
        TimeProvider? timeProvider = null)
    {
        streamId = streamId ?? GenerateStreamId();
        IEvDbSchoolStreamFactory factory = CreateFactory(storageAdapter, timeProvider);
        var stream = factory.Create(streamId);
        return stream;
    }

    #endregion // GivenLocalStream

    #region GivenFactoryForStoredStreamWithEvents

    public static (IEvDbSchoolStreamFactory Factory, string StreamId) GivenFactoryForStoredStreamWithEvents(
        ITestOutputHelper output,
        IEvDbStorageAdapter storageAdapter,
        string? streamId = null,
        Action<IEvDbStorageAdapter, string>? mockGetAsyncResult = null,
        bool withEvents = true,
        TimeProvider? timeProvider = null)
    {
        streamId = streamId ?? GenerateStreamId();
        IEvDbSchoolStreamFactory factory = CreateFactory(storageAdapter, timeProvider);

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

    public static async Task<IEvDbSchoolStream> WhenGetStreamAsync(this (IEvDbSchoolStreamFactory Factory, string StreamId) input)
    {
        var (factory, streamId) = input;
        var result = await factory.GetAsync(streamId);
        return result;
    }

    #endregion // WhenGetStreamAsync

    #region WhenAddingPendingEvents

    public static async Task<IEvDbSchoolStream> GivenAddingPendingEventsAsync(
                    this Task<IEvDbSchoolStream> streamTask,
                    int numOfGrades = NUM_OF_GRADES)
    {
        var stream = await streamTask;
        return stream.WhenAddingPendingEvents(numOfGrades);
    }

    public static IEvDbSchoolStream WhenAddingPendingEvents(
                    this IEvDbSchoolStream stream,
                    int numOfGrades = NUM_OF_GRADES)
    {
        if (stream.StoreOffset == -1)
            stream.EnlistStudent();
        stream.WhenAddGrades(numOfGrades: numOfGrades);
        return stream;

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

        A.CallTo(() => storageAdapter.GetEventsAsync(A<EvDbStreamCursor>.Ignored, A<CancellationToken>.Ignored))
        .ReturnsLazily((EvDbStreamCursor cursor, CancellationToken ct) =>
        {
            if (withEvents)
            {
                List<EvDbEvent> storedEvents = CreateStoredEvents(
                                factory,
                                streamId,
                                serializerOptions,
                                cursor.Offset);
                return storedEvents.ToAsync();
            }
            return Array.Empty<EvDbEvent>().ToAsync();
        });

        return factory;
    }

    #endregion // SetupMockGetAsync

    #region GivenStreamWithStaleEvents

    public static IEvDbSchoolStream GivenStreamWithStaleEvents(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output)
    {

        A.CallTo(() => storageAdapter.SaveStreamAsync(A<IEvDbStreamStoreData>.Ignored, A<CancellationToken>.Ignored))
                .Throws<OCCException>();
        var stream = storageAdapter.GivenLocalStreamWithPendingEvents(output, 6);
        return stream;
    }

    #endregion // GivenStreamWithStaleEvents

    #region CreateStoredEvents

    private static List<EvDbEvent> CreateStoredEvents(
        IEvDbSchoolStreamFactory factory,
        string streamId,
        JsonSerializerOptions? serializerOptions,
        long initOffset = 0)
    {
        List<EvDbEvent> storedEvents = new();
        StudentEnlistedEvent student = Steps.CreateStudentEnlistedEvent();
        bool withEnlisted = initOffset == 0;
        if (withEnlisted)
        {
            EvDbStreamCursor cursor = new(factory.PartitionAddress, streamId, initOffset);
            var e = student.CreateEvent(cursor, options: serializerOptions);
            storedEvents.Add(e);
            initOffset++;
        }
        for (int i = 0; i < 3; i++)
        {
            EvDbStreamCursor cursor = new(factory.PartitionAddress, streamId, initOffset + i);
            double gradeValue = DefaultGradeStrategy(i + 1);
            var grade = new StudentReceivedGradeEvent(20992, student.Student.Id, gradeValue);
            var gradeEvent = grade.CreateEvent(cursor, options: serializerOptions);
            storedEvents.Add(gradeEvent);
        }
        return storedEvents;
    }

    #endregion // CreateStoredEvents

    #region GivenNoSnapshot

    public static (IEvDbSchoolStreamFactory Factory, string StreamId) GivenNoSnapshot(
        this (IEvDbSchoolStreamFactory Factory, string StreamId) input,
        IEvDbStorageAdapter storageAdapter)
    {
        A.CallTo(() => storageAdapter.GetSnapshotAsync(
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
                StudentStatsView.ViewName => 61,
                _ => throw new NotImplementedException()
            });
    }

    #endregion // GivenHavingSnapshotsWithDifferentOffset

    #region GivenHavingSnapshot

    public static (IEvDbSchoolStreamFactory Factory, string StreamId) GivenHavingSnapshots(
        this (IEvDbSchoolStreamFactory Factory, string StreamId) input,
        IEvDbStorageAdapter storageAdapter,
        Func<string, long> getSnapshotOffset)
    {
        A.CallTo(() => storageAdapter.GetSnapshotAsync(
                    A<EvDbViewAddress>.That.Matches(a => a.ViewName == StudentStatsView.ViewName), A<CancellationToken>.Ignored))
            .ReturnsLazily<EvDbStoredSnapshot>(() =>
                {
                    long offset = getSnapshotOffset(StudentStatsView.ViewName);
                    var snapshot = CreateStudentStatsSnapshot(offset, input.Factory.Options);
                    return snapshot;
                });

        A.CallTo(() => storageAdapter.GetSnapshotAsync(
                    A<EvDbViewAddress>.That.Matches(a => a.ViewName == StatsView.ViewName), A<CancellationToken>.Ignored))
            .ReturnsLazily<EvDbStoredSnapshot>(() =>
                {
                    long offset = getSnapshotOffset(StatsView.ViewName);
                    var snapshot = CreateStatsSnapshot(offset, input.Factory.Options);
                    return snapshot;
                });

        A.CallTo(() => storageAdapter.GetSnapshotAsync(
                    A<EvDbViewAddress>.That.Matches(a => a.ViewName == MinEventIntervalSecondsView.ViewName), A<CancellationToken>.Ignored))
            .ReturnsLazily<EvDbStoredSnapshot>(() =>
                {
                    long offset = getSnapshotOffset(MinEventIntervalSecondsView.ViewName);
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

    public static IEvDbSchoolStream EnlistStudent(this IEvDbSchoolStream stream,
        int studentId = 2202,
        string studentName = "Lora")
    {
        var studentEnlisted = CreateStudentEnlistedEvent();
        stream.Add(studentEnlisted);
        return stream;
    }

    #endregion // EnlistStudent

    #region WhenAddGrades

    public static async Task<IEvDbSchoolStream> GivenAddGradesAsync(this Task<IEvDbSchoolStream> streamAsync,
        int testId = TEST_ID,
        int studentId = STUDENT_ID,
        int numOfGrades = NUM_OF_GRADES,
        Func<int, double>? gradeStrategy = null)
    {
        var stream = await streamAsync;
        var result = stream.GivenAddGrades(testId, studentId, numOfGrades, gradeStrategy);
        return result;
    }

    public static IEvDbSchoolStream GivenAddGrades(this IEvDbSchoolStream stream,
        int testId = TEST_ID,
        int studentId = STUDENT_ID,
        int numOfGrades = NUM_OF_GRADES,
        Func<int, double>? gradeStrategy = null) =>
            stream.WhenAddGrades(testId, studentId, numOfGrades, gradeStrategy);

    public static IEvDbSchoolStream WhenAddGrades(this IEvDbSchoolStream stream,
        int testId = TEST_ID,
        int studentId = STUDENT_ID,
        int numOfGrades = NUM_OF_GRADES,
        Func<int, double>? gradeStrategy = null)
    {
        gradeStrategy = gradeStrategy ?? DefaultGradeStrategy;
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(testId, studentId, gradeStrategy(i));
            stream.Add(grade);
        }

        return stream;
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
                        .WhenGetStreamAsync();
        return stream;

    }

    #endregion // GivenStreamRetrievedFromStore

    #region GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset

    public static async Task<IEvDbSchoolStream> GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output)
    {
        var stream = await Steps
                        .GivenFactoryForStoredStreamWithEvents(output, storageAdapter, withEvents: true)
                        .GivenHavingSnapshotsWithDifferentOffset(storageAdapter)
                        .WhenGetStreamAsync();
        return stream;

    }

    #endregion // GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset

    #region GivenLocalStreamWithPendingEvents

    public static IEvDbSchoolStream GivenLocalStreamWithPendingEvents(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output,
        int numOfGrades = NUM_OF_GRADES,
        TimeProvider? timeProvider = null)
    {
        return GivenLocalStream(storageAdapter, timeProvider: timeProvider)
                            .WhenAddingPendingEvents(numOfGrades);
    }

    #endregion // GivenLocalStreamWithPendingEvents

    #region WhenStreamIsSaved

    public static Task<IEvDbSchoolStream> GivenStreamIsSavedAsync(
        this IEvDbSchoolStream stream) => stream.WhenStreamIsSavedAsync();

    public async static Task<IEvDbSchoolStream> WhenStreamIsSavedAsync(
        this IEvDbSchoolStream stream)
    {
        await stream.SaveAsync();
        return stream;
    }

    public async static Task<IEvDbSchoolStream> WhenStreamIsSavedAsync(
        this Task<IEvDbSchoolStream> streamAsync)
    {
        var stream = await streamAsync;
        await stream.SaveAsync();
        return stream;
    }

    #endregion // WhenStreamIsSavedAsync

    #region GivenStoredEvents

    public static async Task<IEvDbSchoolStream> GivenStoredEvents(
        this IEvDbStorageAdapter storageAdapter,
        ITestOutputHelper output)
    {
        IEvDbSchoolStream stream = await storageAdapter.GivenLocalStreamWithPendingEvents(output)
                     .WhenStreamIsSavedAsync();
        return stream;
    }

    #endregion // GivenStoredEvents
}
