namespace EvDb.StructuresValidation.Abstractions.Views;

public readonly record struct CustomerEntityModelState
{
    public Person Person { get; init; }
}