using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("{StreamAddress}, {PartitionAddress}, {Offset}")]
public readonly record struct SnapshotSaveParameter(
                    Guid Id,
                    string Domain,
                    string Partition,
                    string StreamId,
                    string ViewName,
                    long Offset,
                    byte[] State)
{
    public SnapshotSaveParameter(EvDbViewAddress viewAddress, EvDbStoredSnapshot storedSnapshot)
        : this(
              Guid.NewGuid(),   
              viewAddress.Domain,
              viewAddress.Partition,
              viewAddress.StreamId,
              viewAddress.ViewName,
              storedSnapshot.Offset,
              storedSnapshot.State)
    {

    }
};
