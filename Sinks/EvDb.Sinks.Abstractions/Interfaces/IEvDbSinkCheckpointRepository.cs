using EvDb.Core;

namespace EvDb.Sinks;

// TBD: do we need a kind parameter (event, outbox, snapshot) in order to ensure they don't collide?

internal interface IEvDbSinkCheckpointRepository
{
    public Task<EvDbSinkCheckpoint> GetCheckpointAsync(
                                                       EvDbSinkTarget sinkName,
                                                       EvDbStorageContextData context);
    public Task StoreCheckpointAsync(EvDbSinkTarget sinkName,
                                     EvDbStorageContextData context);
}
