namespace EvDb.Sinks;

// TBD: do we need a kind parameter (event, outbox, snapshot) in order to ensure they don't collide?

public interface IEvDbMessagesSinkProcessor
{
    Task StartMessagesSinkAsync(CancellationToken cancellationToken = default);
}

