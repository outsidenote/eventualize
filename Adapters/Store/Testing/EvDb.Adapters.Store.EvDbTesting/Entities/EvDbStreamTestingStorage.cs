using EvDb.Core;
using System.Collections.Immutable;

namespace Microsoft.Extensions.DependencyInjection;

public class EvDbStreamTestingStorage
{
    public IImmutableDictionary<EvDbStreamAddress, EvDbTestingStreamData> Store { get; init; } = ImmutableDictionary<EvDbStreamAddress, EvDbTestingStreamData>.Empty;
}
