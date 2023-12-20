using System.Text.Json;

namespace Eventualize.Core;

public record EventualizeStoredSnapshotData<T>(T Snapshot, long SnapshotSequenceId)
{
    [Obsolete("Only for Dapper")]
    public EventualizeStoredSnapshotData(string Snapshot, long SnapshotSequenceId) :
        this(JsonSerializer.Deserialize<T>(Snapshot), SnapshotSequenceId)
    { 
    }
};