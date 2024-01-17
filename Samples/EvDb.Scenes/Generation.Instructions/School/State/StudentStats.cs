namespace EvDb.UnitTests;

public readonly record struct StudentStats(string Student, double Sum, int Count)
{
    public StudentStats AddGrade(double grade) => this with { Count = Count + 1, Sum = Sum + grade };
}
