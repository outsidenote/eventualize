namespace EvDb.Core.Tests;

using Dapper;
using EvDb.Core.Adapters;
using FakeItEasy;
using LiteDB;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using Xunit.Abstractions;

public class IntegrationTests : IAsyncLifetime
{
    protected readonly IEvDbStorageMigration _storageMigration;
    protected readonly ITestOutputHelper _output;
    protected readonly ILogger _logger = A.Fake<ILogger>();
    protected readonly DbConnection _connection;
    private readonly string _outboxQuery;

    public IntegrationTests(ITestOutputHelper output, StoreType storeType)
    {
        _output = output;
        var context = new EvDbTestStorageContext();
        StorageContext = context;
        _storageMigration = StoreAdapterHelper.CreateStoreMigration(_logger, storeType, context);
        _connection = StoreAdapterHelper.GetConnection(storeType, context);
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

        _outboxQuery =
            $"""
                SELECT
                    {toSnakeCase(nameof(EvDbOutboxRecord.Domain))} as {nameof(EvDbOutboxRecord.Domain)},
                    {toSnakeCase(nameof(EvDbOutboxRecord.Partition))} as {nameof(EvDbOutboxRecord.Partition)},
                    {toSnakeCase(nameof(EvDbOutboxRecord.StreamId))} as {nameof(EvDbOutboxRecord.StreamId)},
                    {toSnakeCase(nameof(EvDbOutboxRecord.Offset))} as {nameof(EvDbOutboxRecord.Offset)},
                    {toSnakeCase(nameof(EvDbOutboxRecord.EventType))} as {nameof(EvDbOutboxRecord.EventType)},
                    {toSnakeCase(nameof(EvDbOutboxRecord.OutboxType))} as {nameof(EvDbOutboxRecord.OutboxType)},
                    {toSnakeCase(nameof(EvDbOutboxRecord.CapturedAt))} as {nameof(EvDbOutboxRecord.CapturedAt)},
                    {toSnakeCase(nameof(EvDbOutboxRecord.CapturedBy))} as {nameof(EvDbOutboxRecord.CapturedBy)},
                    {toSnakeCase(nameof(EvDbOutboxRecord.Payload))} as {nameof(EvDbOutboxRecord.Payload)}                  
                FROM {context}outbox WITH (READCOMMITTEDLOCK)
                ORDER BY {toSnakeCase(nameof(EvDbOutboxRecord.Offset))};
                """;
    }

    public async IAsyncEnumerable<EvDbOutboxEntity> GetOutboxAsync()
    {
        await _connection.OpenAsync();
        DbDataReader reader = await _connection.ExecuteReaderAsync(_outboxQuery);
        var parser = reader.GetRowParser<EvDbOutboxRecord>();
        while (await reader.ReadAsync())
        {
            EvDbOutboxEntity e = parser(reader);
            yield return e;
        }
    }


    public EvDbStorageContext StorageContext { get; }

    public async Task InitializeAsync()
    {
        await _storageMigration.CreateEnvironmentAsync();
    }

    public async Task DisposeAsync()
    {
        await _storageMigration.DisposeAsync();
        await _connection.CloseAsync();
    }
}
