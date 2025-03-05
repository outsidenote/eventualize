// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Cocona;
using EvDb.StructuresValidation.Abstractions;
using EvDb.StructuresValidation.Abstractions.Events;
using EvDb.StructuresValidation.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

public abstract class StreamStructuresBaseTests : BaseIntegrationTests
{
    private readonly IEvDbCustomerEntityModelSingleChannelStream _streamSingle;
    private readonly IEvDbCustomerEntityModelDefaultAndSingleChannelsStream _streamDefaultAndSingle;
    private readonly IEvDbCustomerEntityModelDefaultChannelStream _streamDefault;
    private readonly IEvDbCustomerEntityModelMultiChannelsStream _streamMulti;
    private readonly IEvDbCustomerEntityModelNoChannelsStream _streamNo;

    public StreamStructuresBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        Guid streamId = Guid.NewGuid();
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddEvDb()
                .AddCustomerEntityModelSingleChannelStreamFactory(c => c.ChooseStoreAdapter(storeType), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, AlternativeContext));
        services.AddEvDb()
                .AddCustomerEntityModelDefaultAndSingleChannelsStreamFactory(c => c.ChooseStoreAdapter(storeType), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, AlternativeContext));
        services.AddEvDb()
                .AddCustomerEntityModelDefaultChannelStreamFactory(c => c.ChooseStoreAdapter(storeType), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, AlternativeContext));
        services.AddEvDb()
                .AddCustomerEntityModelMultiChannelsStreamFactory(c => c.ChooseStoreAdapter(storeType), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, AlternativeContext));
        services.AddEvDb()
                .AddCustomerEntityModelNoChannelsStreamFactory(c => c.ChooseStoreAdapter(storeType), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, AlternativeContext));
        var sp = services.BuildServiceProvider();
        IEvDbCustomerEntityModelSingleChannelStreamFactory factorySingle = sp.GetRequiredService<IEvDbCustomerEntityModelSingleChannelStreamFactory>();
        IEvDbCustomerEntityModelDefaultAndSingleChannelsStreamFactory factoryDefaultAndSingle = sp.GetRequiredService<IEvDbCustomerEntityModelDefaultAndSingleChannelsStreamFactory>();
        IEvDbCustomerEntityModelDefaultChannelStreamFactory factoryDefault = sp.GetRequiredService<IEvDbCustomerEntityModelDefaultChannelStreamFactory>();
        IEvDbCustomerEntityModelMultiChannelsStreamFactory factoryMulti = sp.GetRequiredService<IEvDbCustomerEntityModelMultiChannelsStreamFactory>();
        IEvDbCustomerEntityModelNoChannelsStreamFactory factoryNo = sp.GetRequiredService<IEvDbCustomerEntityModelNoChannelsStreamFactory>();
        _streamSingle = factorySingle.Create(streamId);
        _streamDefaultAndSingle = factoryDefaultAndSingle.Create(streamId);
        _streamDefault = factoryDefault.Create(streamId);
        _streamMulti = factoryMulti.Create(streamId);
        _streamNo = factoryNo.Create(streamId);
    }

    [Fact]
    public async Task Stream_Simple_Outbox_NoChannels_Succeed()
    {
        var emailValidated = new EmailValidatedEvent("bnaya@somewhere.com", true);
        await _streamNo.AddAsync(emailValidated);

        await _streamNo.StoreAsync();


        Assert.Equal(1, _streamNo.StoredOffset);

        var outboxEnumerable = await GetOutboxAsync(EvDbShardName.Default).ToEnumerableAsync();
        var outbox = outboxEnumerable.ToArray();
        Assert.Single(outbox);
        Assert.Equal(PersonChangedMessage.PAYLOAD_TYPE, outbox[0].MessageType);
    }

    [Fact]
    public async Task Stream_Simple_Outbox_SingleChannel_Succeed()
    {
        var emailValidated = new EmailValidatedEvent("bnaya@somewhere.com", true);
        await _streamSingle.AddAsync(emailValidated);

        await _streamSingle.StoreAsync();


        Assert.Equal(1, _streamSingle.StoredOffset);

        var outboxEnumerable = await GetOutboxAsync(EvDbShardName.Default).ToEnumerableAsync();
        var outbox = outboxEnumerable.ToArray();
        Assert.Single(outbox);
        Assert.Equal(PersonChangedSingleChannelMessage.PAYLOAD_TYPE, outbox[0].MessageType);
    }

    [Fact]
    public async Task Stream_Simple_Outbox_DefaultAndSingleChannels_Succeed()
    {
        var emailValidated = new EmailValidatedEvent("bnaya@somewhere.com", true);
        await _streamDefaultAndSingle.AddAsync(emailValidated);

        await _streamDefaultAndSingle.StoreAsync();


        Assert.Equal(1, _streamDefaultAndSingle.StoredOffset);

        var outboxEnumerable = await GetOutboxAsync(EvDbShardName.Default).ToEnumerableAsync();
        var outbox = outboxEnumerable.ToArray();
        Assert.Single(outbox);
        Assert.Equal(PersonChangedDefaultAndSingleChannelsMessage.PAYLOAD_TYPE, outbox[0].MessageType);
        Assert.Equal(OutboxChannels02.Channel1, outbox[0].Channel);
    }

    [Fact]
    public async Task Stream_Simple_Outbox_DefaultChannel_Succeed()
    {
        var emailValidated = new EmailValidatedEvent("bnaya@somewhere.com", true);
        await _streamDefault.AddAsync(emailValidated);

        await _streamDefault.StoreAsync();


        Assert.Equal(1, _streamDefault.StoredOffset);

        var outboxEnumerable = await GetOutboxAsync(EvDbShardName.Default).ToEnumerableAsync();
        var outbox = outboxEnumerable.ToArray();
        Assert.Single(outbox);
        Assert.Equal(PersonChangedDefaultChannelMessage.PAYLOAD_TYPE, outbox[0].MessageType);
        Assert.Equal(EvDbChannelName.Default, outbox[0].Channel);
    }

    [Fact]
    public async Task Stream_Simple_Outbox_MultiChannels_Succeed()
    {
        var emailValidated = new EmailValidatedEvent("bnaya@somewhere.com", true);
        await _streamMulti.AddAsync(emailValidated);

        await _streamMulti.StoreAsync();


        Assert.Equal(1, _streamMulti.StoredOffset);

        var outboxEnumerable = await GetOutboxAsync(EvDbShardName.Default).ToEnumerableAsync();
        var outbox = outboxEnumerable.ToArray();
        Assert.Equal(2, outbox.Length);
        Assert.All(outbox, m => Assert.Equal(PersonChangedMultiChannelsMessage.PAYLOAD_TYPE, m.MessageType));
        Assert.Contains(outbox, m => m.Channel == OutboxChannels02.Channel1);
        Assert.Contains(outbox, m => m.Channel == OutboxChannels02.Channel2);
    }
}