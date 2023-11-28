using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;

namespace Core.Repository
{
    public class OCCException<T> : Exception where T : notnull, new()
    {
        public OCCException() { }
        public OCCException(string message) : base(message) { }
        public OCCException(Aggregate<T> aggregate, long storedLastSequenceId) : base(PrepareMessageFromAggregate(aggregate, storedLastSequenceId))
        {
        }
        private static string PrepareMessageFromAggregate<K>(Aggregate<K> aggregate, long lastStoredSequenceId) where K : notnull, new()
        {
            return $"AggregateType='{aggregate.AggregateType.Name}', Id={aggregate.Id}, aggregateLastStoredSequenceId={aggregate.LastStoredSequenceId}, ActualLastStoredSequenceId={lastStoredSequenceId}";
        }
    }
}