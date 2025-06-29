using System.Collections.Immutable;

namespace EvDb.DemoWebApi;

public readonly partial record struct Status(string Name, int Rate)
{
    public static readonly Status Empty = new Status(string.Empty, 0);

    public IImmutableList<string> Comments { get; init; } = ImmutableArray<string>.Empty;
}


