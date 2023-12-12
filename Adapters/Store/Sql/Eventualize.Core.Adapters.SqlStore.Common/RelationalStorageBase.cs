using Microsoft.Data.SqlClient;
using Namotion.Reflection;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace Eventualize.Core.Adapters.SqlStore;

public abstract class RelationalStorageBase: IDisposable, IAsyncDisposable
{
    protected readonly DbConnection _connection;
    protected readonly StorageAdapterContextId _contextId;
    protected readonly Task _init;

    public RelationalStorageBase(
        Func<DbConnection> factory,
        StorageAdapterContextId? contextId = null)
    {
        _contextId = contextId ?? new StorageAdapterContextId();
        _connection = factory() ?? throw new ArgumentNullException($"{nameof(factory)}.Connection");
        _init = InitAsync();
    }

    private async Task InitAsync()
    {
        await _connection.OpenAsync();
    }

    void IDisposable.Dispose()
    {
        DisposeAsync(true ).Wait();
        GC.SuppressFinalize(this);
    }

    protected  async virtual Task DisposeAsync(bool disposed)
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
