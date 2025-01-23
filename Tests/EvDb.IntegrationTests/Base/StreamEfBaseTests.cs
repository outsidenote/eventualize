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
using EvDb.IntegrationTests.EF.Events;
using EvDb.IntegrationTests.EF.Factories;
using System.Security.AccessControl;
using EvDb.IntegrationTests.EF;

public abstract class StreamEfBaseTests : IntegrationTests
{
    private readonly IEvDbPersonFactory _factory;

    public StreamEfBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddEvDb()
                .AddPersonFactory(c => c.ChooseStoreAdapter(storeType), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, AlternativeContext))
                .ForTyped(c => c.UsePersonSqlServerForEvDbSnapshot());
        var sp = services.BuildServiceProvider();
        _factory = sp.GetRequiredService<IEvDbPersonFactory>();
    }

    [Fact]
    public async Task Stream_Ef_TypedSnapshot_Succeed()
    {
        // MinEventsBetweenSnapshots = 3

        const int id = 10;
        IEvDbPerson stream = _factory.Create(id);
        PersonNameChanged personName = new(id, "Nora");
        await stream.AddAsync(personName);

        #region Validation

        Assert.Equal(id, stream.Views.Typed.Id);
        Assert.Equal(personName.Name, stream.Views.Typed.Name);
        Assert.Empty(stream.Views.Typed.Emails);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(id, stream.Views.Typed.Id);
        Assert.Equal(personName.Name, stream.Views.Typed.Name);
        Assert.Empty(stream.Views.Typed.Emails);

        #endregion //  Validation

        var email1 = new PersonEmailAdded(id, "nora@gmail.com", "personal");
        await stream.AddAsync(email1);

        #region Validation

        Assert.Single(stream.Views.Typed.Emails);
        Assert.Null(stream.Views.Typed.Address);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Single(stream.Views.Typed.Emails);
        Assert.Null(stream.Views.Typed.Address);

        #endregion //  Validation

        var email2 = new PersonEmailAdded(id, "nora@work.com", "work");
        await stream.AddAsync(email2);

        #region Validation

        Assert.Equal(2, stream.Views.Typed.Emails.Length);
        Assert.Null(stream.Views.Typed.Address);

        #endregion //  Validation

        await stream.StoreAsync(); // Stream & Snapshot
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(2, stream.Views.Typed.Emails.Length);
        Assert.Null(stream.Views.Typed.Address);

        #endregion //  Validation

        var address = new Address("US", "Anytown", "123 Main St");
        var addressEvent = new PersonAddressChanged(id, address);
        await stream.AddAsync(addressEvent);

        #region Validation

        Assert.Equal(2, stream.Views.Typed.Emails.Length);
        Assert.Equal(address, stream.Views.Typed.Address);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(id, stream.Views.Typed.Id);
        Assert.Equal(personName.Name, stream.Views.Typed.Name);
        Assert.Equal(2, stream.Views.Typed.Emails.Length);
        Assert.Contains("nora@gmail.com", stream.Views.Typed.Emails.Select(e => e.Value));
        Assert.Contains("nora@work.com", stream.Views.Typed.Emails.Select(e => e.Value));
        Assert.Equal(address, stream.Views.Typed.Address);

        #endregion //  Validation
    }

    //[Fact]
    //public async Task Stream_Ef_UntypedSnapshot_Succeed()
    //{
    //    // MinEventsBetweenSnapshots = 3

    //    const int id = 10;
    //    IEvDbPerson stream = _factory.Create(id);
    //    PersonNameChanged personName = new(id, "Nora");
    //    await stream.AddAsync(personName);

    //    #region Validation

    //    Assert.Equal(id, stream.Views.Untyped.Id);
    //    Assert.Equal(personName.Name, stream.Views.Untyped.Name);
    //    Assert.Empty(stream.Views.Untyped.Emails);

    //    #endregion //  Validation

    //    await stream.StoreAsync(); // Only Stream, Snapshot not created
    //    stream = await _factory.GetAsync(id);

    //    #region Validation

    //    Assert.Equal(id, stream.Views.Untyped.Id);
    //    Assert.Equal(personName.Name, stream.Views.Untyped.Name);
    //    Assert.Empty(stream.Views.Untyped.Emails);

    //    #endregion //  Validation

    //    var email1 = new PersonEmailAdded(id, "nora@gmail.com", "personal");
    //    await stream.AddAsync(email1);

    //    await stream.StoreAsync(); // Only Stream, Snapshot not created
    //    stream = await _factory.GetAsync(id);

    //    #region Validation

    //    Assert.Single(stream.Views.Untyped.Emails);
    //    Assert.Null(stream.Views.Untyped.Address);

    //    #endregion //  Validation

    //    var email2 = new PersonEmailAdded(id, "nora@work.com", "work");
    //    await stream.AddAsync(email2);

    //    await stream.StoreAsync(); // Stream & Snapshot
    //    stream = await _factory.GetAsync(id);

    //    #region Validation

    //    Assert.Equal(2, stream.Views.Untyped.Emails.Length);
    //    Assert.Null(stream.Views.Untyped.Address);

    //    #endregion //  Validation

    //    var address = new Address("US", "Anytown", "123 Main St");
    //    var addressEvent = new PersonAddressChanged(id, address);
    //    await stream.AddAsync(addressEvent);

    //    #region Validation

    //    Assert.Equal(2, stream.Views.Untyped.Emails.Length);
    //    Assert.Equal(address, stream.Views.Untyped.Address);

    //    #endregion //  Validation

    //    await stream.StoreAsync(); // Only Stream, Snapshot not created
    //    stream = await _factory.GetAsync(id);

    //    #region Validation

    //    Assert.Equal(id, stream.Views.Untyped.Id);
    //    Assert.Equal(personName.Name, stream.Views.Untyped.Name);
    //    Assert.Equal(2, stream.Views.Untyped.Emails.Length);
    //    Assert.Contains("nora@gmail.com", stream.Views.Untyped.Emails.Select(e => e.Value));
    //    Assert.Contains("nora@work.com", stream.Views.Untyped.Emails.Select(e => e.Value));
    //    Assert.Equal(address, stream.Views.Untyped.Address);

    //    #endregion //  Validation    
    //}
}