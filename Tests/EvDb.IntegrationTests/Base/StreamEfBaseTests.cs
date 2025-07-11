// Ignore Spelling: Sql

#if TYPED_STORAGE_ADAPTER

namespace EvDb.Core.Tests;

using Cocona;
using EvDb.IntegrationTests.EF;
using EvDb.IntegrationTests.EF.Events;
using EvDb.IntegrationTests.EF.Factories;
using EvDb.IntegrationTests.EF.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

public abstract class StreamEfBaseTests : BaseIntegrationTests
{
    private readonly IEvDbPersonFactory _factory;

    public StreamEfBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        switch (storeType)
        {
            case StoreType.SqlServer:
                services.AddSqlServerDbContextFactory<PersonContext>();
                break;
            case StoreType.Postgres:
                services.AddPostgresDbContextFactory<PersonContext>();
                break;
        }
        services.AddEvDb()
                .AddPersonFactory(c => c.ChooseStoreAdapter(storeType, TestingStreamStore), StorageContext)
                .DefaultSnapshotConfiguration(c =>
                {
                    c.ChooseSnapshotAdapter(storeType, TestingStreamStore, AlternativeContext);
                    switch (storeType)
                    {
                        case StoreType.SqlServer:
                            c.UseTypedSqlServerForEvDbSnapshot<EvDbPersonStorageStreamAdapterFactory>(
                                       c => c.ViewName == PersonTyped2View.ViewName);
                            break;
                        case StoreType.Postgres:
                            c.UseTypedPostgresForEvDbSnapshot<EvDbPersonStorageStreamAdapterFactory>(
                                    c => c.ViewName == PersonTyped2View.ViewName);
                            break;
                    }
                })
                .ForTyped(c =>
                {
                    switch (storeType)
                    {
                        case StoreType.SqlServer:
                            c.UseTypedSqlServerForEvDbSnapshot<EvDbPersonStorageStreamAdapterFactory>();
                            break;
                        case StoreType.Postgres:
                            c.UseTypedPostgresForEvDbSnapshot<EvDbPersonStorageStreamAdapterFactory>();
                            break;
                    }
                });
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
        await stream.AppendAsync(personName);

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
        await stream.AppendAsync(email1);

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
        await stream.AppendAsync(email2);

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
        await stream.AppendAsync(addressEvent);

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

        var birthday = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10));
        var birthdayEvent = new PersonBirthdayChanged(id, birthday);
        await stream.AppendAsync(birthdayEvent);

        #region Validation

        Assert.Equal(birthday, stream.Views.Typed.Birthday);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(birthday, stream.Views.Typed.Birthday);

        #endregion //  Validation

        var email3 = new PersonEmailRemoved(id, "nora@work.com");
        await stream.AppendAsync(email3);
        var email4 = new PersonEmailCategoryUpdated(id, "nora@gmail.com", "family");
        await stream.AppendAsync(email4);

        #region Validation

        Assert.Single(stream.Views.Typed.Emails);
        Assert.DoesNotContain(stream.Views.Typed.Emails, e => e.Value == email3.Email);
        Assert.Equal(email4.Category, stream.Views.Typed.Emails.First().Category);

        #endregion //  Validation

        await stream.StoreAsync(); // Stream & Snapshot
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Single(stream.Views.Typed.Emails);
        Assert.DoesNotContain(stream.Views.Typed.Emails, e => e.Value == email3.Email);
        Assert.Equal(email4.Category, stream.Views.Typed.Emails.First().Category);
        Assert.Equal(address, stream.Views.Typed.Address);

        #endregion //  Validation
    }

    [Fact]
    public async Task Stream_Ef_UntypedSnapshot_Succeed()
    {
        // MinEventsBetweenSnapshots = 3

        const int id = 10;
        IEvDbPerson stream = _factory.Create(id);
        PersonNameChanged personName = new(id, "Nora");
        await stream.AppendAsync(personName);

        #region Validation

        Assert.Equal(id, stream.Views.Typed.Id);
        Assert.Equal(personName.Name, stream.Views.Typed.Name);
        Assert.Empty(stream.Views.Untyped.Emails);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(id, stream.Views.Untyped.Id);
        Assert.Equal(personName.Name, stream.Views.Untyped.Name);
        Assert.Empty(stream.Views.Untyped.Emails);

        #endregion //  Validation

        var email1 = new PersonEmailAdded(id, "nora@gmail.com", "personal");
        await stream.AppendAsync(email1);

        #region Validation

        Assert.Single(stream.Views.Untyped.Emails);
        Assert.Null(stream.Views.Untyped.Address);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Single(stream.Views.Untyped.Emails);
        Assert.Null(stream.Views.Untyped.Address);

        #endregion //  Validation

        var email2 = new PersonEmailAdded(id, "nora@work.com", "work");
        await stream.AppendAsync(email2);

        #region Validation

        Assert.Equal(2, stream.Views.Untyped.Emails.Length);
        Assert.Null(stream.Views.Untyped.Address);

        #endregion //  Validation

        await stream.StoreAsync(); // Stream & Snapshot
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(2, stream.Views.Untyped.Emails.Length);
        Assert.Null(stream.Views.Untyped.Address);

        #endregion //  Validation

        var address = new Address("US", "Anytown", "123 Main St");
        var addressEvent = new PersonAddressChanged(id, address);
        await stream.AppendAsync(addressEvent);

        #region Validation

        Assert.Equal(2, stream.Views.Untyped.Emails.Length);
        Assert.Equal(address, stream.Views.Untyped.Address);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(id, stream.Views.Untyped.Id);
        Assert.Equal(personName.Name, stream.Views.Untyped.Name);
        Assert.Equal(2, stream.Views.Untyped.Emails.Length);
        Assert.Contains("nora@gmail.com", stream.Views.Untyped.Emails.Select(e => e.Value));
        Assert.Contains("nora@work.com", stream.Views.Untyped.Emails.Select(e => e.Value));
        Assert.Equal(address, stream.Views.Untyped.Address);

        #endregion //  Validation

        var birthday = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10));
        var birthdayEvent = new PersonBirthdayChanged(id, birthday);
        await stream.AppendAsync(birthdayEvent);

        #region Validation

        Assert.Equal(birthday, stream.Views.Untyped.Birthday);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(birthday, stream.Views.Untyped.Birthday);

        #endregion //  Validation

        var email3 = new PersonEmailRemoved(id, "nora@work.com");
        await stream.AppendAsync(email3);
        var email4 = new PersonEmailCategoryUpdated(id, "nora@gmail.com", "family");
        await stream.AppendAsync(email4);

        #region Validation

        Assert.Single(stream.Views.Untyped.Emails);
        Assert.DoesNotContain(stream.Views.Untyped.Emails, e => e.Value == email3.Email);
        Assert.Equal(email4.Category, stream.Views.Untyped.Emails.First().Category);

        #endregion //  Validation

        await stream.StoreAsync(); // Stream & Snapshot
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Single(stream.Views.Untyped.Emails);
        Assert.DoesNotContain(stream.Views.Untyped.Emails, e => e.Value == email3.Email);
        Assert.Equal(email4.Category, stream.Views.Untyped.Emails.First().Category);
        Assert.Equal(address, stream.Views.Untyped.Address);

        #endregion //  Validation
    }

    [Fact]
    public async Task Stream_Ef_Typed2Snapshot_Succeed()
    {
        // MinEventsBetweenSnapshots = 3

        const int id = 10;
        IEvDbPerson stream = _factory.Create(id);
        PersonNameChanged personName = new(id, "Nora");
        await stream.AppendAsync(personName);

        #region Validation

        Assert.Equal(id, stream.Views.Typed.Id);
        Assert.Equal(personName.Name, stream.Views.Typed.Name);
        Assert.Empty(stream.Views.Typed2.Emails);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(id, stream.Views.Typed2.Id);
        Assert.Equal(personName.Name, stream.Views.Typed2.Name);
        Assert.Empty(stream.Views.Typed2.Emails);

        #endregion //  Validation

        var email1 = new PersonEmailAdded(id, "nora@gmail.com", "personal");
        await stream.AppendAsync(email1);

        #region Validation

        Assert.Single(stream.Views.Typed2.Emails);
        Assert.Null(stream.Views.Typed2.Address);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Single(stream.Views.Typed2.Emails);
        Assert.Null(stream.Views.Typed2.Address);

        #endregion //  Validation

        var email2 = new PersonEmailAdded(id, "nora@work.com", "work");
        await stream.AppendAsync(email2);

        #region Validation

        Assert.Equal(2, stream.Views.Typed2.Emails.Length);
        Assert.Null(stream.Views.Typed2.Address);

        #endregion //  Validation

        await stream.StoreAsync(); // Stream & Snapshot
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(2, stream.Views.Typed2.Emails.Length);
        Assert.Null(stream.Views.Typed2.Address);

        #endregion //  Validation

        var address = new Address("US", "Anytown", "123 Main St");
        var addressEvent = new PersonAddressChanged(id, address);
        await stream.AppendAsync(addressEvent);

        #region Validation

        Assert.Equal(2, stream.Views.Typed2.Emails.Length);
        Assert.Equal(address, stream.Views.Typed2.Address);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(id, stream.Views.Typed2.Id);
        Assert.Equal(personName.Name, stream.Views.Typed2.Name);
        Assert.Equal(2, stream.Views.Typed2.Emails.Length);
        Assert.Contains("nora@gmail.com", stream.Views.Typed2.Emails.Select(e => e.Value));
        Assert.Contains("nora@work.com", stream.Views.Typed2.Emails.Select(e => e.Value));
        Assert.Equal(address, stream.Views.Typed2.Address);

        #endregion //  Validation

        var birthday = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10));
        var birthdayEvent = new PersonBirthdayChanged(id, birthday);
        await stream.AppendAsync(birthdayEvent);

        #region Validation

        Assert.Equal(birthday, stream.Views.Typed2.Birthday);

        #endregion //  Validation

        await stream.StoreAsync(); // Only Stream, Snapshot not created
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Equal(birthday, stream.Views.Typed2.Birthday);

        #endregion //  Validation

        var email3 = new PersonEmailRemoved(id, "nora@work.com");
        await stream.AppendAsync(email3);
        var email4 = new PersonEmailCategoryUpdated(id, "nora@gmail.com", "family");
        await stream.AppendAsync(email4);

        #region Validation

        Assert.Single(stream.Views.Typed2.Emails);
        Assert.DoesNotContain(stream.Views.Typed2.Emails, e => e.Value == email3.Email);
        Assert.Equal(email4.Category, stream.Views.Typed2.Emails.First().Category);

        #endregion //  Validation

        await stream.StoreAsync(); // Stream & Snapshot
        stream = await _factory.GetAsync(id);

        #region Validation

        Assert.Single(stream.Views.Typed2.Emails);
        Assert.DoesNotContain(stream.Views.Typed2.Emails, e => e.Value == email3.Email);
        Assert.Equal(email4.Category, stream.Views.Typed2.Emails.First().Category);
        Assert.Equal(address, stream.Views.Typed2.Address);

        #endregion //  Validation
    }
}
#endif // TYPED_STORAGE_ADAPTER