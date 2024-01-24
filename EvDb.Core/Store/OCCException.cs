#pragma warning disable S1133 // Deprecated code should be removed

namespace EvDb.Core
{
    public class OCCException : Exception
    {
        public OCCException() { }
        [Obsolete("Shouldn't be used directly, used by the serialization", true)]
        public OCCException(string message) : base(message) { }
        public OCCException(IEvDbStreamStore streamStore) : this(streamStore, -1)
        {
        }
        public OCCException(IEvDbStreamStore streamStore, long storedLastOffset) : base(PrepareMessageFromAggregate(streamStore, storedLastOffset))
        {
        }
        private static string PrepareMessageFromAggregate(IEvDbStreamStore streamStore, long lastStoredOffset)
        {
            return $"{streamStore.StreamAddress}, StreamLastStoredOffset={streamStore.LastStoredOffset}, ActualLastStoredOffset={lastStoredOffset}";
        }
    }
}