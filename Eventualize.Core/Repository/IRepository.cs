
namespace Eventualize.Core;

public interface IRepository
{
    Task<Aggregate<T>> GetAsync<T>(AggregateType<T> aggregateType, string id) where T : notnull, new();
    Task SaveAsync<T>(Aggregate<T> aggregate) where T : notnull, new();
}