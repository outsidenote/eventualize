namespace EvDb.Core;

public static class AsyncEnumerable<T>
{
    public readonly static IAsyncEnumerable<T> Empty = GetEmpty();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private static async IAsyncEnumerable<T> GetEmpty()
    {
        yield break;
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
