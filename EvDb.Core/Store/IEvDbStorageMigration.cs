namespace EvDb.Core;

public interface IEvDbStorageMigration : IDisposable, IAsyncDisposable
{
    Task CreateEnvironmentAsync(CancellationToken cancellation = default);
    Task DestroyEnvironmentAsync(CancellationToken cancellation = default);
}