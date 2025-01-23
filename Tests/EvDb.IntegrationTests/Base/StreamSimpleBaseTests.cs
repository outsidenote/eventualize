// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Cocona;
using EvDb.Adapters.Store.Postgres;
using EvDb.Adapters.Store.SqlServer;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using System.Text.Json;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;

public abstract class StreamSimpleBaseTests : IntegrationTests
{
    private readonly IEvDbSchoolStream _stream;

    public StreamSimpleBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        Guid streamId = Guid.NewGuid();
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddEvDb()
                .AddSchoolStreamFactory(c => c.ChooseStoreAdapter(storeType), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, AlternativeContext));
        var sp = services.BuildServiceProvider();
        IEvDbSchoolStreamFactory factory = sp.GetRequiredService<IEvDbSchoolStreamFactory>();
        _stream = factory.Create(streamId);

    }

    [Fact]
    public async Task Stream_WithAltSnapshot_Succeed()
    {
        var student =  new StudentEntity(10, "Mikey"); ;
        var studentEnlisted = new StudentEnlistedEvent(student);
        await _stream.AddAsync(studentEnlisted);
        for (int i = 1; i < 4; i++)
        {
            var grade = new StudentReceivedGradeEvent(i, student.Id, i % 2 == 0 ? 80 : 90);
            await _stream.AddAsync(grade);
        }
        await _stream.StoreAsync();


        Assert.Equal(3, _stream.StoredOffset);
        Assert.Equal(3, _stream.Views.ALL.Count);
        Assert.Equal(260, _stream.Views.ALL.Sum);
    }
}