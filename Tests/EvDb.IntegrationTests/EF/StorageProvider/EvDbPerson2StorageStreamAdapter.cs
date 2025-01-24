using EvDb.Core;
using EvDb.IntegrationTests.EF;
using EvDb.IntegrationTests.EF.States;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection;

public class EvDbPerson2StorageStreamAdapter : EvDbTypedStorageStreamAdapter
{
    private readonly IDbContextFactory<Person2Context> _contextFactory;

    public EvDbPerson2StorageStreamAdapter(IDbContextFactory<Person2Context> contextFactory,
                                          IEvDbStorageSnapshotAdapter adapter,
                                          Predicate<EvDbViewAddress>? canHandle = null) : base(adapter, canHandle)
    {
        _contextFactory = contextFactory;
    }

    protected override bool CanHandle<TState>(EvDbViewAddress address) => typeof(TState) == typeof(Person);

    async protected override Task<EvDbStoredSnapshotBase> OnGetSnapshotAsync(EvDbViewAddress viewAddress,
                                                                       EvDbStoredSnapshot metadata,
                                                                       CancellationToken cancellation)
    {
        int personId = JsonSerializer.Deserialize<int>(metadata.State);
        var context = _contextFactory.CreateDbContext();
        PersonEntity? entity = await context.Persons
                                    .Include(p => p.Emails) // Eagerly load the related Emails
                                    .FirstOrDefaultAsync(p => p.Id == personId);

        Person person = entity == null ? new Person() : entity.FromEntity();
        var result = new EvDbStoredSnapshot<Person>(metadata.Offset, person);
        return result;
    }

    async protected override Task<byte[]> OnStoreSnapshotAsync(EvDbStoredSnapshotDataBase data,
                                                               CancellationToken cancellation)
    {
        EvDbStoredSnapshotData<Person> snapshot = (EvDbStoredSnapshotData<Person>)data;
        Person person = snapshot.State;
        PersonEntity state = person.ToEntity();
        state = state with { Id = person.Id };
        var context = _contextFactory.CreateDbContext();
        if (data.StoreOffset == 0)
        {
            await context.Persons.AddAsync(state);
            foreach (var email in person.Emails)
            {
                var entry = email.ToEntity(person.Id);
                await context.Emails.AddAsync(entry);
            }
        }
        else
        {
            context.Persons.Update(state);
            await UpdateEmails();
        }
        await context.SaveChangesAsync();
        byte[] id = JsonSerializer.SerializeToUtf8Bytes(state.Id);
        return id;

        #region UpdateEmails

        async Task UpdateEmails()
        {
            // Load existing emails for the person from the database
            EmailEntity[] existingEmails = await context.Emails
                .Where(e => e.PersonId == state.Id)
                .ToArrayAsync();
            var existingEmailValues = existingEmails
                                .Select(e => e.Value)
                                .ToHashSet();
            var candidateEmailValues = person.Emails
                                .Select(e => e.Value)
                                .ToHashSet();


            foreach (var item in existingEmails)
            {
                if (candidateEmailValues.Contains(item.Value))
                {
                    var entry = person.Emails
                                        .First(e => e.Value == item.Value)
                                        .ToEntity(state.Id);
                    if (entry != item)
                        context.Entry(item).CurrentValues.SetValues(entry);
                }
                else
                    context.Emails.Entry(item).State = EntityState.Deleted;
            }

            var added = person.Emails
                .Where(e => !existingEmailValues.Contains(e.Value))
                .Select(e => e.ToEntity(person.Id));

            foreach (var item in added)
            {
                context.Emails.Add(item);
            }
        }

        #endregion //  UpdateEmails
    }

}
