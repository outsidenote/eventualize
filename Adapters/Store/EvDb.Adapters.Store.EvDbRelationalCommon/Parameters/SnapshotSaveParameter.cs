using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("{StreamAddress}, {PartitionAddress}, {Offset}")]
public readonly record struct SnapshotSaveParameter(
                    string Domain,
                    string Partition,
                    string StreamId,
                    string ViewName,
                    long Offset,
                    string State)
{
    public SnapshotSaveParameter(EvDbViewAddress viewAddress, EvDbStoredSnapshot storedSnapshot)
        : this(
              viewAddress.Domain,
              viewAddress.Partition,
              viewAddress.StreamId,
              viewAddress.ViewName,
              storedSnapshot.Offset,
              storedSnapshot.State)
    {

    }
};
