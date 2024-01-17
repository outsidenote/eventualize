namespace EvDb.Core.Tests;

using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Scenes;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Xunit.Abstractions;

using STATE_TYPE = System.Collections.Immutable.IImmutableDictionary<int, EvDb.UnitTests.StudentStats>;

internal static class Steps
{
    public static string GenerateStreamId() => $"test-stream-{Guid.NewGuid():N}";

    #region CreateFactory

    private static ISchoolFactory CreateFactory(
        ITestOutputHelper output,
        IEvDbStorageAdapter? storageAdapter)
    {
        storageAdapter = storageAdapter;
        ServiceCollection services = new();
        services.AddSingleton(storageAdapter);
        services.AddSingleton<ISchoolFactory, SchoolFactory>();
        var sp = services.BuildServiceProvider();
        ISchoolFactory factory = sp.GetRequiredService<ISchoolFactory>();
        return factory;
    }

    #endregion // CreateFactory

    #region GivenLocalAggerate

    public static ISchool GivenLocalAggerate(
        ITestOutputHelper output,
        IEvDbStorageAdapter? storageAdapter,
        string? streamId = null)
    {
        streamId = streamId ?? GenerateStreamId();
        ISchoolFactory factory = CreateFactory(output, storageAdapter);
        var aggregate = factory.Create(streamId);
        return aggregate;
    }

    #endregion // GivenLocalAggerate

    #region GivenFactoryForStoredStreamWithEvents

    public static (ISchoolFactory Factory, string StreamId) GivenFactoryForStoredStreamWithEvents(
        ITestOutputHelper output,
        IEvDbStorageAdapter storageAdapter,
        string? streamId=  null,
        Action<IEvDbStorageAdapter, string>? mockGetAsyncResult = null,
        bool withEvents = true)
    {
        streamId = streamId ?? GenerateStreamId();
        ISchoolFactory factory = CreateFactory(output, storageAdapter);

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

    public static async Task<ISchool> WhenGetAggregateAsync(this (ISchoolFactory Factory, string StreamId) input)
    {
        var (factory, streamId) = input;
        var result = await factory.GetAsync(streamId);
        return result;
    }

    #endregion // WhenGetAggregateAsync

    #region WhenAddingPendingEvents

    public static ISchool WhenAddingPendingEvents(this ISchool aggregate)
    {
        aggregate.EnlistStudent()
                  .WhenAddGrades();
        return aggregate;

    }

    #endregion // WhenAddingPendingEvents

    #region SetupMockGetAsync

    public static ISchoolFactory SetupMockGetAsync(
        this ISchoolFactory factory,
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

    #region CreateStoredEvents

    private static List<EvDbStoredEvent> CreateStoredEvents(
        ISchoolFactory factory,
        string streamId,
        JsonSerializerOptions? serializerOptions,
        long initOffset = 0)
    {
        List<EvDbStoredEvent> storedEvents = [];
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

    public static (ISchoolFactory Factory, string StreamId) GivenNoSnapshot(
        this (ISchoolFactory Factory, string StreamId) input,
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

    public static (ISchoolFactory Factory, string StreamId) GivenHavingSnapshot(
        this (ISchoolFactory Factory, string StreamId) input,
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
            new EvDbStoredSnapshot<STATE_TYPE> (state, cursor);      
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

    public static ISchool EnlistStudent(this ISchool aggregate,
        int studentId = 2202,
        string studentName = "Lora")
    {
        var studentEnlisted = CreateStudentEnlistedEvent();
        aggregate.Add(studentEnlisted);
        return aggregate;
    }

    #endregion // EnlistStudent

    #region WhenAddGrades

    public static ISchool WhenAddGrades(this ISchool aggregate,
        int testId = 6628,
        int studentId = 2202,
        int numOfGrades = 3,
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

    public static async Task<ISchool> GivenAggregateRetrievedFromStore(
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
}
