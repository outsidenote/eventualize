// Ignore Spelling: Testing

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static EvDb.Core.Adapters.StoreTelemetry;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Adapters.Store.Testing;

/// <summary>
/// Testing streamStorage adapter that handles event streams, snapshots, and outbox messages.
/// </summary>
internal sealed class EvDbTestingStorageAdapter : IEvDbStorageStreamAdapter, IEvDbStorageSnapshotAdapter
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly EvDbStreamTestingStorage _streamStorage;
    private readonly EvDbSnapshotTestingStorage _snapshotStorage;
    private readonly IImmutableList<IEvDbOutboxTransformer> _transformers;
    private const string DATABASE_TYPE = "Testing";

    #region Ctor

    public EvDbTestingStorageAdapter(
                        EvDbStorageContext storageContext,
                        IEnumerable<IEvDbOutboxTransformer> transformers,
                        EvDbStreamTestingStorage? streamStorage = null,
                        EvDbSnapshotTestingStorage? snapshotStorage = null)
    {
        _streamStorage = streamStorage ?? new EvDbStreamTestingStorage();
        _snapshotStorage = snapshotStorage ?? new EvDbSnapshotTestingStorage();
        _transformers = transformers.ToImmutableArray();
    }

    #endregion //  Ctor

    #region GetEventsAsync

    async IAsyncEnumerable<EvDbEvent> IEvDbStorageStreamAdapter.GetEventsAsync(
        EvDbStreamCursor streamCursor,
        [EnumeratorCancellation] CancellationToken cancellation)
    {
        throw new NotImplementedException("GetEventsAsync is not implemented in EvDbTestingStorageAdapter.");
    }

    #endregion //  GetEventsAsync

    #region GetLastEventAsync

    async Task<long> IEvDbStorageStreamAdapter.GetLastOffsetAsync(
        EvDbStreamAddress address,
        CancellationToken cancellation)
    {
        throw new NotImplementedException("GetLastOffsetAsync is not implemented in EvDbTestingStorageAdapter.");
    }

    #endregion //  GetLastOffsetAsync

    #region StoreStreamAsync

    async Task<Core.StreamStoreAffected> IEvDbStorageStreamAdapter.StoreStreamAsync(IImmutableList<EvDbEvent> events,
                                                                              IImmutableList<EvDbMessage> messages,
                                                                              CancellationToken cancellation)
    {
        if (events.Count == 0)
            return StreamStoreAffected.Empty;

        EvDbEvent firstEvent = events.FirstOrDefault();
        var cursor = firstEvent.StreamCursor;
        EvDbStreamAddress address = cursor;

        if(!_streamStorage.Store.TryGetValue(address, out EvDbTestingStreamData? storedStream))
            storedStream = EvDbTestingStreamData.Empty;

        var lasStoredtEvent = storedStream.Events[^1];
        if(firstEvent.StreamCursor.Offset - 1 != lasStoredtEvent.StreamCursor.Offset)
        {
            throw new OCCException(lasStoredtEvent.StreamCursor);
        }

        // Interlocked.Exchange(ref storedStream, new EvDbTestingStreamData(storedStream.Events.AddRange(events), storedStream.Messages.AddRange(messages)));
        // occ use interlocked

        throw new NotImplementedException("StoreStreamAsync is not implemented in EvDbTestingStorageAdapter.");
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
        throw new NotImplementedException("GetSnapshotAsync is not implemented in EvDbTestingStorageAdapter.");
    }

    #endregion //  GetSnapshotAsync

    #region StoreSnapshotAsync

    /// <summary>
    /// Stores a snapshot.
    /// </summary>
    async Task IEvDbStorageSnapshotAdapter.StoreSnapshotAsync(EvDbStoredSnapshotData snapshotData, CancellationToken cancellation)
    {
        throw new NotImplementedException("StoreSnapshotAsync is not implemented in EvDbTestingStorageAdapter.");
    }

    #endregion //  StoreSnapshotAsync
}
