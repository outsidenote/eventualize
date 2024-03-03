namespace EvDb.Core.Tests;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class IntegrationTests : IAsyncLifetime
{
    protected readonly IEvDbStorageAdapter _storageAdapter;
    protected readonly IEvDbStorageMigration _storageMigration;
    protected readonly ITestOutputHelper _output;
    protected readonly ILogger _logger = A.Fake<ILogger>();
    public IntegrationTests(ITestOutputHelper output, StoreType storeType)
    {
        _output = output;
        var context = new EvDbTestStorageContext();
        _storageAdapter = StoreAdapterHelper.CreateStoreAdapter(_logger, storeType, context);
        _storageMigration = StoreAdapterHelper.CreateStoreMigration(_logger, storeType, context);
    }

    public async Task InitializeAsync()
    {
        await _storageMigration.CreateEnvironmentAsync();
    }

    public async Task DisposeAsync()
    {
        await _storageMigration.DisposeAsync();
    }
}
