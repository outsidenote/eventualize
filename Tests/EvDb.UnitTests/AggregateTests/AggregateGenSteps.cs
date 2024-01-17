﻿namespace EvDb.Core.Tests;

using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Scenes;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Xunit.Abstractions;

using STATE_TYPE = System.Collections.Immutable.IImmutableDictionary<int, EvDb.UnitTests.StudentStats>;
using TRY_GET_SNAPSHOT_RETURN_TYPE = Task<EvDbStoredSnapshot<System.Collections.Immutable.IImmutableDictionary<int, EvDb.UnitTests.StudentStats>>?>;

internal static class AggregateGenSteps
{
    public static string GenerateStreamId() => $"test-stream-{Guid.NewGuid():N}";

    #region CreateFactory

    private static ISchoolFactory CreateFactory(
        ITestOutputHelper output,
        IEvDbStorageAdapter? storageAdapter = null)
    {
        storageAdapter = storageAdapter ?? A.Fake<IEvDbStorageAdapter>();
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
        IEvDbStorageAdapter? storageAdapter = null,
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
        string? streamId=  null,
        Action<IEvDbStorageAdapter, string>? mockGetAsyncResult = null)
    {
        streamId = streamId ?? GenerateStreamId();
        var storageAdapter = A.Fake<IEvDbStorageAdapter>();
        ISchoolFactory factory = CreateFactory(output, storageAdapter);

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
        aggregate.AddStudent()
                  .AddGrades();
        return aggregate;

    }

    #endregion // WhenAddingPendingEvents

    #region MockGetAsync

    public static ISchoolFactory MockGetAsync(
        this ISchoolFactory factory,
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

    #region MockTryGetSnapshotAsync

    public static ISchoolFactory MockTryGetSnapshotAsync(
        this ISchoolFactory factory,
        IEvDbStorageAdapter storageAdapter,
        string streamId = "1234",
        JsonSerializerOptions? serializerOptions = null)
    {
        #region List<EvDbStoredEvent> storedEvents = ...

        List<EvDbStoredEvent> storedEvents = [];

        EvDbStreamAddress address = new(factory.Partition, streamId);
        EvDbStreamCursor cursor = new(address, 0);
        StudentEnlistedEvent studentEnlisted = AggregateGenSteps.CreateStudent();
        var e = EvDbEventFactory.Create(studentEnlisted, serializerOptions);
        var eStored = new EvDbStoredEvent(e, cursor);
        storedEvents.Add(eStored);

        for (int i = 1; i <= 3; i++)
        {
            EvDbStreamCursor csr = new(address, i);
            var grade = new StudentReceivedGradeEvent(20992, studentEnlisted.Student.Id, 30 * i);
            var gradeEvent = EvDbEventFactory.Create(grade, serializerOptions);
            var gradeStoreEvent = new EvDbStoredEvent(gradeEvent, csr);
            storedEvents.Add(gradeStoreEvent);
        }

        #endregion // List<EvDbStoredEvent> storedEvents = ...


        var ct = A.CallTo(() => storageAdapter.TryGetSnapshotAsync<STATE_TYPE>(
            A<EvDbSnapshotId>.Ignored, A<CancellationToken>.Ignored));
        ct.ReturnsLazily<TRY_GET_SNAPSHOT_RETURN_TYPE>(async () =>
        {
            var student = studentEnlisted.Student;
            var stat = new StudentStats(student.Name, 70, 10);
            EvDbSnapshotCursor cursor = new(address, "Stats", 10);
            STATE_TYPE state = ImmutableDictionary<int, StudentStats>.Empty.Add(student.Id, stat);

            EvDbStoredSnapshot<STATE_TYPE>? result;
            await Task.Yield();
            result = new EvDbStoredSnapshot<STATE_TYPE> (state, cursor);
            
            return result;
        });

        return factory;
    }

    #endregion // MockTryGetSnapshotAsync

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

    public static ISchool AddStudent(this ISchool aggregate,
        int studentId = 2202,
        string studentName = "Lora")
    {
        var studentEnlisted = CreateStudent();
        aggregate.Add(studentEnlisted);
        return aggregate;
    }

    #endregion // AddStudent

    #region AddGrades

    private static ISchool AddGrades(this ISchool aggregate,
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
