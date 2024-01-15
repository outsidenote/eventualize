namespace EvDb.Core.Tests;

using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Scenes;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Xunit.Abstractions;

internal static class AggregateGenSteps
{
    public static string GenerateStreamId() => $"test-stream-{Guid.NewGuid():N}";

    #region CreateFactory

    private static IStudentAvgFactory CreateFactory(
        ITestOutputHelper output,
        IEvDbStorageAdapter? storageAdapter = null)
    {
        storageAdapter = storageAdapter ?? A.Fake<IEvDbStorageAdapter>();
        ServiceCollection services = new();
        services.AddSingleton(storageAdapter);
        services.AddSingleton<IStudentAvgFactory, StudentAvgFactory>();
        var sp = services.BuildServiceProvider();
        IStudentAvgFactory factory = sp.GetRequiredService<IStudentAvgFactory>();
        return factory;
    }

    #endregion // CreateFactory

    #region GivenLocalAggerate

    public static IStudentAvg GivenLocalAggerate(
        ITestOutputHelper output,
        IEvDbStorageAdapter? storageAdapter = null,
        string? streamId = null)
    {
        streamId = streamId ?? GenerateStreamId();
        IStudentAvgFactory factory = CreateFactory(output, storageAdapter);
        var aggregate = factory.Create(streamId);
        return aggregate;
    }

    #endregion // GivenLocalAggerate

    #region GivenFactoryForStoredStreamWithEvents

    public static (IStudentAvgFactory Factory, string StreamId) GivenFactoryForStoredStreamWithEvents(
        ITestOutputHelper output,
        string? streamId=  null,
        Action<IEvDbStorageAdapter, string>? mockGetAsyncResult = null)
    {
        streamId = streamId ?? GenerateStreamId();
        var storageAdapter = A.Fake<IEvDbStorageAdapter>();
        IStudentAvgFactory factory = CreateFactory(output, storageAdapter);

        if (mockGetAsyncResult == null)
        {
            factory.MockGetAsync(storageAdapter, streamId);
        }
        else
        {
            mockGetAsyncResult.Invoke(storageAdapter, streamId);
        }
        return (factory, streamId);
    }

    #endregion // GivenFactoryForStoredStreamWithEvents

    #region WhenGetAggregateAsync

    public static async Task<IStudentAvg> WhenGetAggregateAsync(this (IStudentAvgFactory Factory, string StreamId) input)
    {
        var (factory, streamId) = input;
        var result = await factory.GetAsync(streamId);
        return result;
    }

    #endregion // WhenGetAggregateAsync

    #region WhenAddingPendingEvents

    public static IStudentAvg WhenAddingPendingEvents(this IStudentAvg aggregate)
    {
        aggregate.AddStudent()
                  .AddGrades();
        return aggregate;

    }

    #endregion // WhenAddingPendingEvents

    #region MockGetAsync

    public static IStudentAvgFactory MockGetAsync(
        this IStudentAvgFactory factory,
        IEvDbStorageAdapter storageAdapter,
        string streamId = "1234",
        JsonSerializerOptions? serializerOptions = null)
    {
        #region List<EvDbStoredEvent> storedEvents = ...

        List<EvDbStoredEvent> storedEvents = [];

        EvDbStreamCursor cursor = new(factory.Partition, streamId, 0);
        StudentEnlistedEvent student = AggregateGenSteps.CreateStudent();
        var e = EvDbEventFactory.Create(student, serializerOptions);
        var eStored = new EvDbStoredEvent(e, cursor);
        storedEvents.Add(eStored);

        for (int i = 1; i <= 3; i++)
        {
            EvDbStreamCursor csr = new(factory.Partition, streamId, i);
            var grade = new StudentReceivedGradeEvent(20992, student.Student.Id, 30 * i);
            var gradeEvent = EvDbEventFactory.Create(grade, serializerOptions);
            var gradeStoreEvent = new EvDbStoredEvent(gradeEvent, csr);
            storedEvents.Add(gradeStoreEvent);
        }

        #endregion // List<EvDbStoredEvent> storedEvents = ...

        A.CallTo(() => storageAdapter.GetAsync(A<EvDbStreamCursor>.Ignored, A<CancellationToken>.Ignored))
    .ReturnsLazily(() => storedEvents.ToAsync());


        return factory;
    }

    #endregion // MockGetAsync

    #region CreateStudent

    public static StudentEnlistedEvent CreateStudent(
        int studentId = 2202,
        string studentName = "Lora")
    {
        var lora = new StudentEntity(studentId, studentName);
        var studentEnlisted = new StudentEnlistedEvent(lora);
        return studentEnlisted;
    }

    #endregion // CreateStudent

    #region AddStudent

    public static IStudentAvg AddStudent(this IStudentAvg aggregate,
        int studentId = 2202,
        string studentName = "Lora")
    {
        var studentEnlisted = CreateStudent();
        aggregate.Add(studentEnlisted);
        return aggregate;
    }

    #endregion // AddStudent

    #region AddGrades

    private static IStudentAvg AddGrades(this IStudentAvg aggregate,
        int testId = 6628,
        int studentId = 2202,
        int numOfGrades = 3,
        Func<int, double>? gradeStrategy = null)
    {
        gradeStrategy = gradeStrategy ?? ((i) => i * 3);
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(testId, studentId, gradeStrategy(i));
            aggregate.Add(grade);
        }

        return aggregate;
    }

    #endregion // AddGrades
}
