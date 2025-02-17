// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Cocona;
using EvDb.Scenes;
using EvDb.StructuresValidation.Abstractions;
using EvDb.StructuresValidation.Abstractions.Events;
using EvDb.StructuresValidation.Repositories;
using EvDb.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

public abstract class StreamStructuresBaseTests : IntegrationTests
{
    private readonly IEvDbCustomerEntityModelSingleChannelStream _stream;

    public StreamStructuresBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        Guid streamId = Guid.NewGuid();
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddEvDb()
                .AddCustomerEntityModelSingleChannelStreamFactory(c => c.ChooseStoreAdapter(storeType), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, AlternativeContext));
        var sp = services.BuildServiceProvider();
        IEvDbCustomerEntityModelSingleChannelStreamFactory factory = sp.GetRequiredService<IEvDbCustomerEntityModelSingleChannelStreamFactory>();
        _stream = factory.Create(streamId);

    }

    [Fact]
    public async Task Stream_Simple_Outbox_Succeed()
    {
        var emailValidated = new EmailValidatedEvent("bnaya@somewhere.com", true);
        await _stream.AddAsync(emailValidated);

        await _stream.StoreAsync();


        Assert.Equal(1, _stream.StoredOffset);

        var outboxEnumerable =  await base.GetOutboxAsync(EvDbShardName.Default).ToEnumerableAsync();
        var outbox = outboxEnumerable.ToArray();
        Assert.Single(outbox);
        Assert.Equal(PersonChangedMessage.PAYLOAD_TYPE, outbox[0].MessageType);
    }
}