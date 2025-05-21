using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Abstractions.Views;

[EvDbViewType<CustomerEntityModelState, ICustomerEntityModelEvents>("customer-entity-model")]
public partial class CustomerEntityModelView
{
    protected override CustomerEntityModelState DefaultState { get; } = new CustomerEntityModelState();

    protected override CustomerEntityModelState Apply(CustomerEntityModelState state,
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
