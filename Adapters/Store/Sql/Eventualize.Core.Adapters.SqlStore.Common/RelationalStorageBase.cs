using System.Data.Common;

namespace Eventualize.Core.Adapters.SqlStore;

public abstract class RelationalStorageBase : IDisposable, IAsyncDisposable
{
    protected readonly DbConnection _connection;
    protected readonly StorageContext _contextId;
    protected readonly Task _init;

    public RelationalStorageBase(
        Func<DbConnection> factory,
        StorageContext? contextId = null)
    {
        _contextId = contextId ?? StorageContext.Default;
        _connection = factory() ?? throw new ArgumentNullException($"{nameof(factory)}.Connection");
        _init = InitAsync();
    }

    private async Task InitAsync()
    {
        await _connection.OpenAsync();
    }

    void IDisposable.Dispose()
    {
        DisposeAsync(true).Wait();
        GC.SuppressFinalize(this);
    }

    protected async virtual Task DisposeAsync(bool disposed)
    {
        await _connection.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await DisposeAsync(true);
    }

    ~RelationalStorageBase()
    {
        DisposeAsync(false).Wait();
    }
}
