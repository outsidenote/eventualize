using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core;
public class EventualizeFoldingLogicBuilder
{
    public static EventualizeFoldingLogicBuilder<T> Create<T>() where T : notnull, new()
    {
        return new EventualizeFoldingLogicBuilder<T>();
    }
}

public record EventualizeFoldingLogicBuilder<T> where T : notnull, new()
{
    public IImmutableDictionary<string, IFoldingFunction<T>> Mapping { get; private set; } =
                    ImmutableDictionary<string, IFoldingFunction<T>>.Empty;

    internal EventualizeFoldingLogicBuilder(){}

    public EventualizeFoldingLogicBuilder<T> AddMapping(string eventType, IFoldingFunction<T> foldingFunction)
    {
        var mapping =  Mapping.Add(eventType, foldingFunction);
        return this with { Mapping = mapping };
    }

    public EventualizeFoldingLogic<T> Build()
    {
        return new EventualizeFoldingLogic<T>(Mapping);
    }

}
