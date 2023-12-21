using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Eventualize.Core.Adapters;

public abstract class RelationalStorageBase : IDisposable, IAsyncDisposable
{
    protected readonly DbConnection _connection;
    protected readonly Task _init;
    protected readonly ILogger _logger;

    public RelationalStorageBase(
        ILogger logger,
        Func<DbConnection> factory)
    {
        _connection = factory() ?? throw new ArgumentNullException($"{nameof(factory)}.Connection");
        _init = InitAsync();
        _logger = logger;
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
