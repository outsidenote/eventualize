using System.Collections.Concurrent;

namespace EvDb.DemoWebApi;

public class State
{
    public ConcurrentDictionary<string, string> Properties { get; set; } = new();
}
