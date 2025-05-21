using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("{RootAddress}{StreamId}:{ViewName}, Offset = {Offset}")]
public readonly record struct SnapshotSaveParameter(
                    Guid Id,
                    string RootAddress,
                    string StreamId,
                    string ViewName,
                    long Offset,
                    byte[] State)
{
    public SnapshotSaveParameter(EvDbViewAddress viewAddress, EvDbStoredSnapshot storedSnapshot)
        : this(
              Guid.NewGuid(),
              viewAddress.RootAddress,
              viewAddress.StreamId,
              viewAddress.ViewName,
              storedSnapshot.Offset,
              storedSnapshot.State)
    {

    }
};
