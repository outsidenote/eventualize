namespace EvDb.Core.Tests;

public static class TestHelper
{
    public static async IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> self)
    {
        foreach (var item in self)
        {
            await Task.Yield();
            yield return item;
        }
    }

#pragma warning disable S5034 // "ValueTask" should be consumed correctly
    public static async Task<ICollection<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> self)
    {
        var list = new List<T>();
        await foreach (var item in self)
        {
            list.Add(item);
        }
        return list;
    }
#pragma warning restore S5034 // "ValueTask" should be consumed correctly
}