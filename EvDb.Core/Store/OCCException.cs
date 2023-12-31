#pragma warning disable S1133 // Deprecated code should be removed

namespace EvDb.Core
{
    public class OCCException<T> : Exception where T : notnull, new()
    {
        [Obsolete("Shouldn't be used directly, used by the serialization", true)]
        public OCCException() { }
        [Obsolete("Shouldn't be used directly, used by the serialization", true)]
        public OCCException(string message) : base(message) { }
        public OCCException(EvDbAggregate<T> aggregate) : this(aggregate, -1)
        {
        }
        public OCCException(EvDbAggregate<T> aggregate, long storedLastOffset) : base(PrepareMessageFromAggregate(aggregate, storedLastOffset))
        {
        }
        private static string PrepareMessageFromAggregate<K>(EvDbAggregate<K> aggregate, long lastStoredOffset) where K : notnull, new()
        {
            return $"AggregateType={aggregate.AggregateType}, StreamUri='{aggregate.StreamUri}', aggregateLastStoredOffset={aggregate.LastStoredOffset}, ActualLastStoredOffset={lastStoredOffset}";
        }
    }
}