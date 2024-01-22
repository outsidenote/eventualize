namespace EvDb.UnitTests;

public readonly record struct Stats(double Sum, int Count)
{
    public Stats AddGrade(double grade) => this with { Count = Count + 1, Sum = Sum + grade };
}
