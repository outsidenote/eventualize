// Ignore Spelling: Testing

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EvDb.Adapters.Store.Testing;

/// <summary>
/// Testing streamStorage adapter that handles event streams, snapshots, and outbox messages.
/// </summary>
internal sealed class EvDbTestingStorageAdapter : IEvDbStorageStreamAdapter, IEvDbStorageSnapshotAdapter
{
    private readonly EvDbStreamTestingStorage _streamStorage;
    private EvDbSnapshotTestingStorage _snapshotStorage;

    #region Ctor

    public EvDbTestingStorageAdapter(
                        EvDbStorageContext storageContext,
                        IEnumerable<IEvDbOutboxTransformer> transformers,
                        EvDbStreamTestingStorage? streamStorage = null,
                        EvDbSnapshotTestingStorage? snapshotStorage = null)
    {
        _streamStorage = streamStorage ?? new EvDbStreamTestingStorage();
        _snapshotStorage = snapshotStorage ?? new EvDbSnapshotTestingStorage();
    }

    #endregion //  Ctor

    #region GetEventsAsync

    async IAsyncEnumerable<EvDbEvent> IEvDbStorageStreamAdapter.GetEventsAsync(
        EvDbStreamCursor streamCursor,
        [EnumeratorCancellation] CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            yield break;
        await Task.Yield(); // Simulate async operation
        if (_streamStorage.Store.TryGetValue(streamCursor, out EvDbTestingStreamData? storedStream))
        {
            var events = storedStream.Events
                                     .SkipWhile(e => e.StreamCursor.Offset <= streamCursor.Offset);
            foreach (var storedEvent in events)
            {
                yield return storedEvent;
            }
        }
    }

    #endregion //  GetEventsAsync

    #region GetLastEventAsync

    async Task<long> IEvDbStorageStreamAdapter.GetLastOffsetAsync(
        EvDbStreamAddress address,
        CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            return 0;
        await Task.Yield(); // Simulate async operation

        if (!_streamStorage.Store.TryGetValue(address, out EvDbTestingStreamData? storedStream))
            return 0;
        var lastStoredEvent = storedStream.Events[^1];
        return lastStoredEvent.StreamCursor.Offset;
    }

    #endregion //  GetLastOffsetAsync

    #region GetMessagesAsync

    IAsyncEnumerable<EvDbMessageRecord> IEvDbChangeStream.GetMessageRecordsAsync(
                                EvDbShardName shard,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options,
                                CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

    #endregion //  GetMessagesAsync

    #region StoreStreamAsync

    async Task<StreamStoreAffected> IEvDbStorageStreamAdapter.StoreStreamAsync(IImmutableList<EvDbEvent> events,
                                                                              IImmutableList<EvDbMessage> messages,
                                                                              CancellationToken cancellation)
    {
        await Task.Yield(); // Simulate async operation
        if (events.Count == 0)
            return StreamStoreAffected.Empty;

        EvDbTelemetryContextName otel = Activity.Current?.SerializeTelemetryContext() ?? EvDbTelemetryContextName.Empty;
        events = events.Select(e => e with { TelemetryContext = otel })
                       .ToImmutableList();
        messages = messages.Select(e => e with { TelemetryContext = otel })
                       .ToImmutableList();
        EvDbEvent firstEvent = events.FirstOrDefault();
        EvDbStreamCursor cursor = firstEvent.StreamCursor;
        EvDbStreamAddress address = cursor;

        if (!_streamStorage.Store.TryGetValue(address, out EvDbTestingStreamData? storedStream))
            storedStream = EvDbTestingStreamData.Empty;

        if (storedStream.Events.Count != 0)
        {
            var lasStoredtEvent = storedStream.Events[^1];
            if (firstEvent.StreamCursor.Offset - 1 != lasStoredtEvent.StreamCursor.Offset)
            {
                throw new OCCException(lasStoredtEvent.StreamCursor);
            }
        }

        storedStream = storedStream with
        {
            Events = storedStream.Events.AddRange(events),
            Messages = storedStream.Messages.AddRange(messages)
        };

        var store = _streamStorage.Store.Remove(address)
                                        .Add(address, storedStream);
        _streamStorage.SetStore(store, cursor);

        var storeAffected = new StreamStoreAffected(events.Count, ImmutableDictionary<EvDbShardName, int>.Empty);
        return storeAffected;
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
        if (cancellation.IsCancellationRequested)
            return EvDbStoredSnapshot.Empty;
        await Task.Yield(); // Simulate async operation

        if (!_snapshotStorage.Store.TryGetValue(viewAddress, out IImmutableList<EvDbStoredSnapshotData>? storedSnapshot))
            return EvDbStoredSnapshot.Empty;
        var last = storedSnapshot[^1];
        EvDbStoredSnapshot result = new EvDbStoredSnapshot(
            last.Offset,
            last.State);
        return result;
    }

    #endregion //  GetSnapshotAsync

    #region StoreSnapshotAsync

    /// <summary>
    /// Stores a snapshot.
    /// </summary>
    async Task IEvDbStorageSnapshotAdapter.StoreSnapshotAsync(EvDbStoredSnapshotData snapshotData, CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            return;
        await Task.Yield(); // Simulate async operation

        EvDbViewAddress address = snapshotData;
        EvDbSnapshotTestingStorage oldStorage = _snapshotStorage;
        if (!_snapshotStorage.Store.TryGetValue(address, out IImmutableList<EvDbStoredSnapshotData>? storedSnapshot))
            storedSnapshot = ImmutableList<EvDbStoredSnapshotData>.Empty;

        _snapshotStorage = oldStorage with
        {
            Store = oldStorage.Store.Add(address, storedSnapshot.Add(snapshotData))
        };
    }

    #endregion //  StoreSnapshotAsync
}
