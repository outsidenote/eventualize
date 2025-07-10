using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("{StreamType}{StreamId}:{ViewName}, Offset = {Offset}")]
internal readonly record struct SnapshotSaveParameter(
                    Guid Id,
                    string StreamType,
                    string StreamId,
                    string ViewName,
                    long Offset,
                    byte[] State)
{
    public SnapshotSaveParameter(EvDbViewAddress viewAddress, EvDbStoredSnapshotResult storedSnapshot)
        : this(
              Guid.NewGuid(),
              viewAddress.StreamType,
              viewAddress.StreamId,
              viewAddress.ViewName,
              storedSnapshot.Offset,
              storedSnapshot.State)
    {

    }
};
