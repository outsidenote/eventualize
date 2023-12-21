namespace Eventualize.Core;

public interface IEventualizeStorageMigration : IDisposable, IAsyncDisposable
{
    Task CreateTestEnvironmentAsync(CancellationToken cancellation = default);
    Task DestroyTestEnvironmentAsync(CancellationToken cancellation = default);
}