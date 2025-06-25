using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace EvDb.DemoWebApi;

public class State
{
    public ConcurrentDictionary<string, ImmutableList<string>> Comments { get; set; } = new();
}
