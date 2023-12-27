using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core;
public class EventualizeFoldingLogicBuilder<T> where T : notnull, new()
{
    public List<KeyValuePair<string, IFoldingFunction<T>>> Mapping { get; private set; } = [];

    public void AddMapping(string eventType, IFoldingFunction<T> foldingFunction)
    {
        Mapping.Add(KeyValuePair.Create(eventType, foldingFunction));
    }

    public EventualizeFoldingLogic<T> Build()
    {
        var immutableMapping = ImmutableDictionary.CreateRange(Mapping);
        return new EventualizeFoldingLogic<T>(immutableMapping);
    }

}
