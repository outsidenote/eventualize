// using Core.Event;
// namespace Core.Aggregate
// {
// 	public record PendingEvent(string SerializedEvent, Type EventDataType)
// 	{
// 		public static PendingEvent create<EventDataType>(Event<EventDataType> someEvent)
// 		{
// 			return new PendingEvent(someEvent.SerializeEvent(), typeof(EventDataType));
// 		}
// 	};
// }

