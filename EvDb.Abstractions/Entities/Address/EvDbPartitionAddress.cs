namespace EvDb.Core;

public readonly record struct EvDbPartitionAddress(string Domain, string Partition)
{
    public override string ToString() => $"{Domain}:{Partition}";
}