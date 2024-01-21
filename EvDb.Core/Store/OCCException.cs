#pragma warning disable S1133 // Deprecated code should be removed

namespace EvDb.Core
{
    public class OCCException : Exception
    {
        public OCCException() { }
        [Obsolete("Shouldn't be used directly, used by the serialization", true)]
        public OCCException(string message) : base(message) { }
        public OCCException(IEvDbCollectionMeta aggregate) : this(aggregate, -1)
        {
        }
        public OCCException(IEvDbCollectionMeta aggregate, long storedLastOffset) : base(PrepareMessageFromAggregate(aggregate, storedLastOffset))
        {
        }
        private static string PrepareMessageFromAggregate(IEvDbCollectionMeta aggregate, long lastStoredOffset)
        {
            return $"Kind={aggregate.Kind}, StreamId='{aggregate.StreamId}', aggregateLastStoredOffset={aggregate.LastStoredOffset}, ActualLastStoredOffset={lastStoredOffset}";
        }
    }
}