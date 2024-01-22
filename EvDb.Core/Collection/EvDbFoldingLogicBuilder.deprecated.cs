//using System.Collections.Immutable;

//namespace EvDb.Core;
//public class EvDbFoldingLogicBuilder
//{
//    public static EvDbFoldingLogicBuilder<T> Create<T>() where T : notnull, new()
//    {
//        return new EvDbFoldingLogicBuilder<T>();
//    }
//}

//public record EvDbFoldingLogicBuilder<T> where T : notnull, new()
//{
//    public IImmutableDictionary<string, IFoldingFunction<T>> Mapping { get; private set; } =
//                    ImmutableDictionary<string, IFoldingFunction<T>>.Empty;

//    internal EvDbFoldingLogicBuilder() { }

//    public EvDbFoldingLogicBuilder<T> AddMapping(string eventType, IFoldingFunction<T> foldingFunction)
//    {
//        var mapping = Mapping.Add(eventType, foldingFunction);
//        return this with { Mapping = mapping };
//    }

//    public EvDbFoldingLogic<T> Build()
//    {
//        return new EvDbFoldingLogic<T>(Mapping);
//    }

//}
