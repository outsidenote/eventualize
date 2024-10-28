using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Abstractions.Views;

[EvDbViewType<CustomerEntityModelState, ICustomerEntityModelEvents>("customer-entity-model")]
public partial class CustomerEntityModelView
{
    protected override CustomerEntityModelState DefaultState { get; } = new CustomerEntityModelState();

    protected override CustomerEntityModelState Fold(CustomerEntityModelState state,
                                                     EmailValidatedEvent payload,
                                                     IEvDbEventMeta meta)
    {
        return state with
        {
            Person = state.Person with
            {
                Email = payload.Email,
                EmailIsValid = payload.IsValid
            }
        };
    }
}
