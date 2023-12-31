namespace EvDb.Core;

public interface IEvDbStorageMigration : IDisposable, IAsyncDisposable
{
    Task CreateTestEnvironmentAsync(CancellationToken cancellation = default);
    Task DestroyTestEnvironmentAsync(CancellationToken cancellation = default);
}