namespace EvDb.Core;

public readonly record struct EvDbPartitionAddress(EvDbDomainName Domain, EvDbPartitionName Partition)
{
    public override string ToString() => $"{Domain}:{Partition}";
}