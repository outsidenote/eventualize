#pragma warning disable S1133 // Deprecated code should be removed

namespace Eventualize.Core
{
    public class OCCException<T> : Exception where T : notnull, new()
    {
        [Obsolete("Shouldn't be used directly, used by the serialization", true)]
        public OCCException() { }
        [Obsolete("Shouldn't be used directly, used by the serialization", true)]
        public OCCException(string message) : base(message) { }
        public OCCException(EventualizeAggregate<T> aggregate) : this(aggregate, -1)
        {
        }
        public OCCException(EventualizeAggregate<T> aggregate, long storedLastSequenceId) : base(PrepareMessageFromAggregate(aggregate, storedLastSequenceId))
        {
        }
        private static string PrepareMessageFromAggregate<K>(EventualizeAggregate<K> aggregate, long lastStoredSequenceId) where K : notnull, new()
        {
            return $"AggregateType='{aggregate.AggregateType.Name}', Id={aggregate.Id}, aggregateLastStoredSequenceId={aggregate.LastStoredSequenceId}, ActualLastStoredSequenceId={lastStoredSequenceId}";
        }
    }
}