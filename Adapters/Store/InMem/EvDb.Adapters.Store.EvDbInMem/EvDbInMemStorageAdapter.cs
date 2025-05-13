// Ignore Spelling: InMemory

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static EvDb.Core.Adapters.StoreTelemetry;
using System.Collections.Concurrent;

namespace EvDb.Adapters.Store.InMemory;

/// <summary>
/// InMemory storage adapter that handles event streams, snapshots, and outbox messages.
/// </summary>
internal sealed class EvDbInMemoryStorageAdapter : IEvDbStorageStreamAdapter, IEvDbStorageSnapshotAdapter
{
    private readonly ILogger _logger;
    private readonly IImmutableList<IEvDbOutboxTransformer> _transformers;
    private readonly static ActivitySource _trace = StoreTelemetry.Trace;
    private const string DATABASE_TYPE = "InMemory";
    private readonly static ConcurrentDictionary<EvDbStorageContext, InMemoryUnit> _storage = new ConcurrentDictionary<EvDbStorageContext, InMemoryUnit>();

    #region Ctor

    public EvDbInMemoryStorageAdapter(
                        ILogger logger,
                        EvDbStorageContext storageContext,
                        IEnumerable<IEvDbOutboxTransformer> transformers)
    {
        _logger = logger;
        _transformers = transformers.ToImmutableArray();
    }

    #endregion //  Ctor

    #region GetEventsAsync

    async IAsyncEnumerable<EvDbEvent> IEvDbStorageStreamAdapter.GetEventsAsync(
        EvDbStreamCursor streamCursor,
        [EnumeratorCancellation] CancellationToken cancellation)
    {
        throw new NotImplementedException("GetEventsAsync is not implemented in EvDbInMemoryStorageAdapter.");
    }

    #endregion //  GetEventsAsync

    #region GetLastEventAsync

    async Task<long> IEvDbStorageStreamAdapter.GetLastOffsetAsync(
        EvDbStreamAddress address,
        CancellationToken cancellation)
    {
        throw new NotImplementedException("GetLastOffsetAsync is not implemented in EvDbInMemoryStorageAdapter.");
    }

    #endregion //  GetLastOffsetAsync

    #region StoreStreamAsync

    async Task<Core.StreamStoreAffected> IEvDbStorageStreamAdapter.StoreStreamAsync(IImmutableList<EvDbEvent> events,
                                                                              IImmutableList<EvDbMessage> messages,
                                                                              CancellationToken cancellation)
    {
        throw new NotImplementedException("StoreStreamAsync is not implemented in EvDbInMemoryStorageAdapter.");
    }

    #endregion //  StoreStreamAsync

    #region GetSnapshotAsync

    /// <summary>
    /// Retrieves a stored snapshot for the specified view address.
    /// </summary>
    async Task<EvDbStoredSnapshot> IEvDbStorageSnapshotAdapter.GetSnapshotAsync(
                                                EvDbViewAddress viewAddress,
                                                CancellationToken cancellation)
    {
        throw new NotImplementedException("GetSnapshotAsync is not implemented in EvDbInMemoryStorageAdapter.");
    }

    #endregion //  GetSnapshotAsync

    #region StoreSnapshotAsync

    /// <summary>
    /// Stores a snapshot.
    /// </summary>
    async Task IEvDbStorageSnapshotAdapter.StoreSnapshotAsync(EvDbStoredSnapshotData snapshotData, CancellationToken cancellation)
    {
        throw new NotImplementedException("StoreSnapshotAsync is not implemented in EvDbInMemoryStorageAdapter.");
    }

    #endregion //  StoreSnapshotAsync
}
