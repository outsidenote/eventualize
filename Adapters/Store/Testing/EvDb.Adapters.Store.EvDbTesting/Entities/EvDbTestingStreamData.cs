// Ignore Spelling: Sql Testing

using EvDb.Core;
using System.Collections.Immutable;
using EVENTS = System.Collections.Immutable.IImmutableList<EvDb.Core.EvDbEvent>;
using MESSAGES = System.Collections.Immutable.IImmutableList<EvDb.Core.EvDbMessage>;

namespace Microsoft.Extensions.DependencyInjection;

public record EvDbTestingStreamData(EVENTS Events, MESSAGES Messages)
{
    public static EvDbTestingStreamData Empty => new EvDbTestingStreamData(ImmutableList<EvDbEvent>.Empty, ImmutableList<EvDbMessage>.Empty);
}
