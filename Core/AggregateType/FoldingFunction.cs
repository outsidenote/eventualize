using Core.Event;
namespace Core.AggregateType;
public delegate dynamic FoldingFunction(dynamic oldState, dynamic SerializedEvent);