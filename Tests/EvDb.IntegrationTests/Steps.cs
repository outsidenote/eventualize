namespace EvDb.Core.Tests;

using Cocona;
using EvDb.Core.Store.Internals;
using EvDb.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using Scenes;
using Xunit.Abstractions;

internal static class Steps
{
    private const int TEST_ID = 6628;
    private const int STUDENT_ID = 2202;
    private const int NUM_OF_GRADES = 3;

    public static string GenerateStreamId() => $"test-stream-{Guid.NewGuid():N}";

    #region CreateFactory

    public static IEvDbSchoolStreamFactory CreateFactory(
        this EvDbStorageContext storageContext, StoreType storeType)
    {
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddEvDb()
                .AddSchoolStreamFactory(c => c.ChooseStoreAdapter(storeType), storageContext)
                .DefaultSnapshotConfiguration(c =>c.ChooseSnapshotAdapter(storeType));
        var sp = services.BuildServiceProvider();
        IEvDbSchoolStreamFactory factory = sp.GetRequiredService<IEvDbSchoolStreamFactory>();
        return factory;
    }



    #endregion // CreateFactory

    #region GivenLocalAggerate

    public static IEvDbSchoolStream GivenLocalStream(
        this EvDbStorageContext storageContext,
        StoreType storeType,
        string? streamId = null)
    {
        streamId = streamId ?? GenerateStreamId();
        IEvDbSchoolStreamFactory factory = storageContext.CreateFactory(storeType);
        var stream = factory.Create(streamId);
        return stream;
    }

    #endregion // GivenLocalStream

    #region GivenFactoryForStoredStreamWithEvents

    public static async Task<(IEvDbSchoolStreamFactory Factory, string StreamId)> GivenFactoryForStoredStreamWithEvents(
        this EvDbStorageContext storageContext,
        ITestOutputHelper output,
        StoreType storeType,
        string? streamId = null,
        int numOfGrades = NUM_OF_GRADES)
    {
        streamId = streamId ?? GenerateStreamId();
        await storageContext.GivenSavedEventsAsync(output, storeType, streamId, numOfGrades);

        IEvDbSchoolStreamFactory factory = storageContext.CreateFactory(storeType);

        return (factory, streamId);
    }

    #endregion // GivenFactoryForStoredStreamWithEvents

    #region GivenSavedEventsAsync

    public static async Task<IEvDbSchoolStream> GivenSavedEventsAsync(
        this EvDbStorageContext storageContext,
        ITestOutputHelper output,
        StoreType storeType,
        string? streamId,
        int numOfGrades = NUM_OF_GRADES)
    {
        IEvDbSchoolStream stream = await storageContext
                    .GivenLocalStreamWithPendingEvents(storeType ,numOfGrades, streamId)
                    .WhenStreamIsSavedAsync();
        return stream;
    }


    #endregion // GivenSavedEventsAsync

    #region WhenGetAggregateAsync

    public static async Task<IEvDbSchoolStream> WhenGetStreamAsync(this Task<(IEvDbSchoolStreamFactory Factory, string StreamId)> inputTask)
    {
        (IEvDbSchoolStreamFactory Factory, string StreamId) input = await inputTask;
        var result = await input.WhenGetStreamAsync();
        return result;
    }

    public static async Task<IEvDbSchoolStream> WhenGetStreamAsync(this (IEvDbSchoolStreamFactory Factory, string StreamId) input)
    {
        var (factory, streamId) = input;
        var result = await factory.WhenGetStreamAsync(streamId);
        return result;
    }

    public static async Task<IEvDbSchoolStream> WhenGetStreamAsync(this IEvDbSchoolStreamFactory factory, string streamId)
    {
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
        var result = await stream.GivenAddingPendingEvents(numOfGrades);
        return result;
    }

    public static async Task<IEvDbSchoolStream> GivenAddingPendingEvents(
                    this IEvDbSchoolStream stream,
                    int numOfGrades = NUM_OF_GRADES)
    {
        return await stream.WhenAddingPendingEvents(numOfGrades);
    }

    public static async Task<IEvDbSchoolStream> WhenAddingPendingEvents(
                    this IEvDbSchoolStream stream,
                    int numOfGrades = NUM_OF_GRADES)
    {
        if (stream.StoredOffset == 0)
            await stream.EnlistStudent();
        await stream.WhenAddGrades(numOfGrades: numOfGrades);
        return stream;

    }

    #endregion // WhenAddingPendingEvents

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

    public static async Task<IEvDbSchoolStream> EnlistStudent(this IEvDbSchoolStream stream,
        int studentId = 2202,
        string studentName = "Lora")
    {
        var studentEnlisted = CreateStudentEnlistedEvent();
        await stream.AddAsync(studentEnlisted);
        return stream;
    }

    #endregion // EnlistStudent

    #region WhenAddGrades

    public static async Task<IEvDbSchoolStream> WhenAddGrades(this IEvDbSchoolStream stream,
        int testId = TEST_ID,
        int studentId = STUDENT_ID,
        int numOfGrades = NUM_OF_GRADES,
        Func<int, double>? gradeStrategy = null)
    {
        gradeStrategy = gradeStrategy ?? DefaultGradeStrategy;
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(testId, studentId, gradeStrategy(i));
            await stream.AddAsync(grade);
        }

        return stream;
    }

    #endregion // WhenAddGrades

    public static double DefaultGradeStrategy(int i) => i * 30;

    #region GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset

    public static async Task<IEvDbSchoolStream> GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset(
        this EvDbStorageContext storageContext,
        ITestOutputHelper output,
        StoreType storeType,
        string? streamId = null)
    {
        var stream = await storageContext
                        .GivenFactoryForStoredStreamWithEvents(output, storeType, streamId)
                        .WhenGetStreamAsync()
                        .WhenStreamIsSavedAsync()
                        .GivenAddingPendingEventsAsync(1)
                        .WhenStreamIsSavedAsync();
        return stream;

    }

    #endregion // GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset

    #region GivenLocalStreamWithPendingEvents

    public static async Task<IEvDbSchoolStream> GivenLocalStreamWithPendingEvents(
        this EvDbStorageContext storageContext,
        StoreType storeType,
        int numOfGrades = NUM_OF_GRADES,
        string? streamId = null)
    {
        return await storageContext.GivenLocalStream(storeType, streamId)
                            .WhenAddingPendingEvents(numOfGrades);
    }

    #endregion // GivenLocalStreamWithPendingEvents

    #region WhenStreamIsSaved

    public static Task<IEvDbSchoolStream> GivenStreamIsSavedAsync(
        this IEvDbSchoolStream stream) => stream.WhenStreamIsSavedAsync();


    public static async Task<IEvDbSchoolStream> GivenStreamIsSavedAsync(
        this Task<IEvDbSchoolStream> stream) => await stream.WhenStreamIsSavedAsync();

    public async static Task<IEvDbSchoolStream> WhenStreamIsSavedAsync(
        this IEvDbSchoolStream stream)
    {
        await stream.StoreAsync();
        return stream;
    }

    public async static Task<IEvDbSchoolStream> WhenStreamIsSavedAsync(
        this Task<IEvDbSchoolStream> streamAsync)
    {
        var stream = await streamAsync;
        await stream.StoreAsync();
        return stream;
    }

    #endregion // WhenStreamIsSavedAsync
}
