namespace EvDb.Scenes;

public readonly record struct StudentStats(int StudentId, string StudentName, double Sum, int Count)
{
    public StudentStats AddGrade(double grade) => this with { Count = Count + 1, Sum = Sum + grade };
}
