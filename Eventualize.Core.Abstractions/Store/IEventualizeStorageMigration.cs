namespace Eventualize.Core;

public interface IEventualizeStorageMigration : IDisposable, IAsyncDisposable
{
    Task CreateTestEnvironmentAsync();
    Task DestroyTestEnvironmentAsync();
}