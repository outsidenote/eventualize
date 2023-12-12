namespace Eventualize.Core;

public interface IStorageMigration : IDisposable, IAsyncDisposable
{
    Task CreateTestEnvironmentAsync();
    Task DestroyTestEnvironmentAsync();
}