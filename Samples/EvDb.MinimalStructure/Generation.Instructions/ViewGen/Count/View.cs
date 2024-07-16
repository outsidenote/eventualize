using EvDb.Core;
using System.Collections.Concurrent;

namespace EvDb.MinimalStructure.Views.B;

[EvDbViewType<int, IEvents>("count")]
internal partial class View
{
    private readonly ConcurrentDictionary<long, object?> _once = new ConcurrentDictionary<long, object?>();
    protected override int DefaultState { get; } = 0;
    private long _lastOffset = -1;

    protected override int Fold(int state, Event2 payload, IEvDbEventMeta meta)
    {
        //Assert(meta);

        return state + 1;
    }

    protected override int Fold(int state, Event1 payload, IEvDbEventMeta meta)
    {
        //Assert(meta);
        return state + 1;
    }

    private void Assert(IEvDbEventMeta meta)
    {
        var offset = meta.StreamCursor.Offset;
        if (!_once.TryAdd(offset, null))
            throw new InvalidOperationException("Duplicate offset");
        else if (_lastOffset + 1 != offset)
            throw new Exception("offset is out of sync");
        _lastOffset = offset;
    }
}
