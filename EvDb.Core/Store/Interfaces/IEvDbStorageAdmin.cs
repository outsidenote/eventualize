// Ignore Spelling: Admin

namespace EvDb.Core;


public interface IEvDbStorageAdmin : IDisposable, IAsyncDisposable
{
    EvDbAdminQueryTemplates Scripts { get; }
    Task CreateEnvironmentAsync(CancellationToken cancellation = default);
    Task DestroyEnvironmentAsync(CancellationToken cancellation = default);
}