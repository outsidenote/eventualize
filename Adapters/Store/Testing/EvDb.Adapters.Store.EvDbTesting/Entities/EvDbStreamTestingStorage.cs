using EvDb.Core;
using System.Collections.Immutable;
using System.Data.SqlTypes;

namespace Microsoft.Extensions.DependencyInjection;

public class EvDbStreamTestingStorage
{
    private IImmutableDictionary<EvDbStreamAddress, EvDbTestingStreamData> _store  = ImmutableDictionary<EvDbStreamAddress, EvDbTestingStreamData>.Empty;
    public IImmutableDictionary<EvDbStreamAddress, EvDbTestingStreamData> Store => _store;

    internal void SetStore(IImmutableDictionary<EvDbStreamAddress, EvDbTestingStreamData> store, EvDbStreamCursor cursor)
    {
        var origin = Store;
        var actualOrigin = Interlocked.CompareExchange(ref _store, store, origin);
        if(actualOrigin != origin)
        {
            throw new OCCException(cursor);
        }
    }
}
