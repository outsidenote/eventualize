
namespace Eventualize.Core;

public interface IRepository
{
    Task<EventualizeAggregate<T>> GetAsync<T>(EventualizeAggregate<T> aggregate) where T : notnull, new();
    Task SaveAsync<T>(EventualizeAggregate<T> aggregate) where T : notnull, new();
}