using EvDb.Core;
using EvDb.IntegrationTests.EF.Events;
using EvDb.IntegrationTests.EF.States;

namespace EvDb.IntegrationTests.EF.Views;

[EvDbViewType<Person, IPersonEvents>("typed-2-people")]
public partial class PersonTyped2View
{
    protected override Person DefaultState { get; } = new Person() { Emails = Array.Empty<Email>() };
    public override bool ShouldStoreSnapshot(long offsetGapFromLastSave, TimeSpan durationSinceLastSave) => offsetGapFromLastSave > 3;


    protected override Person Apply(Person state, PersonAddressChanged payload, IEvDbEventMeta meta)
    {
        return state with { Id = payload.Id, Address = payload.Address };
    }

    protected override Person Apply(Person state, PersonBirthdayChanged payload, IEvDbEventMeta meta)
    {
        return state with { Id = payload.Id, Birthday = payload.Date };
    }

    protected override Person Apply(Person state, PersonNameChanged payload, IEvDbEventMeta meta)
    {
        return state with { Id = payload.Id, Name = payload.Name };
    }

    protected override Person Apply(Person state, PersonEmailRemoved payload, IEvDbEventMeta meta)
    {
        var emails = state.Emails.Where(e => e.Value != payload.Email).ToArray();
        return state with { Emails = emails };
    }

    protected override Person Apply(Person state, PersonEmailCategoryUpdated payload, IEvDbEventMeta meta)
    {
        var emails = state.Emails.Select(e =>
        {
            if (e.Value == payload.Email)
                return e with { Category = payload.Category };
            return e;
        }).ToArray();

        return state with { Emails = emails };
    }

    protected override Person Apply(Person state, PersonEmailAdded payload, IEvDbEventMeta meta)
    {
        Email email = payload.Email;
        email = email with { Category = payload.Category };
        var emails = state.Emails.ToList();
        emails.Add(email);
        state = state with { Emails = emails.DistinctBy(m => m.Value).ToArray() };
        return state;
    }
}
