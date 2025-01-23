using DeviceDetectorNET.Class.Client;
using EvDb.Core;
using EvDb.IntegrationTests.EF.Events;
using EvDb.IntegrationTests.EF.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.IntegrationTests.EF.Views;

[EvDbViewType<Person, IPersonEvents>("untyped-people")]
public partial class PersonUntypedView
{
    protected override Person DefaultState { get; } = new Person() { Emails = Array.Empty<Email>() };
    public override int MinEventsBetweenSnapshots => 3;


    protected override Person Fold(Person state, PersonAddressChanged payload, IEvDbEventMeta meta)
    {
        return state with { Id = payload.Id, Address = payload.Address };
    }

    protected override Person Fold(Person state, PersonBirthdayChanged payload, IEvDbEventMeta meta)
    {
        return state with { Id = payload.Id, Birthday = payload.Date };
    }

    protected override Person Fold(Person state, PersonNameChanged payload, IEvDbEventMeta meta)
    {
        return state with { Id = payload.Id, Name = payload.Name };
    }

    protected override Person Fold(Person state, PersonEmailRemoved payload, IEvDbEventMeta meta)
    {
        var emails = state.Emails.Where(e => e.Value != payload.Email).ToArray();
        return state with { Emails = emails };
    }

    protected override Person Fold(Person state, PersonEmailCategoryUpdated payload, IEvDbEventMeta meta)
    {
        var emails = state.Emails.Select(e =>
        {
            if (e.Value == payload.Email)
                return e with {  Category = payload.Category };
            return e;
        }).ToArray();

        return state with { Emails = emails };
    }

    protected override Person Fold(Person state, PersonEmailAdded payload, IEvDbEventMeta meta)
    {
        Email email = payload.Email;
        email = email with { Category = payload.Category };
        var emails = state.Emails.ToList();
        emails.Add(email);
        return state with { Emails = emails.DistinctBy(m => m.Value).ToArray() };
    }
}
