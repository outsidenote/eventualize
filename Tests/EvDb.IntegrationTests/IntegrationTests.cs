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
    private readonly string _topicQuery;

    public IntegrationTests(ITestOutputHelper output, StoreType storeType)
    {
        _output = output;
        var context = new EvDbTestStorageContext();
        StorageContext = context;
        _storageMigration = StoreAdapterHelper.CreateStoreMigration(_logger, storeType, context);
        _connection = StoreAdapterHelper.GetConnection(storeType, context);
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

        _topicQuery =
            $"""
                SELECT
                    {toSnakeCase(nameof(EvDbMessageRecord.Domain))} as {nameof(EvDbMessageRecord.Domain)},
                    {toSnakeCase(nameof(EvDbMessageRecord.Partition))} as {nameof(EvDbMessageRecord.Partition)},
                    {toSnakeCase(nameof(EvDbMessageRecord.StreamId))} as {nameof(EvDbMessageRecord.StreamId)},
                    {toSnakeCase(nameof(EvDbMessageRecord.Offset))} as {nameof(EvDbMessageRecord.Offset)},
                    {toSnakeCase(nameof(EvDbMessageRecord.EventType))} as {nameof(EvDbMessageRecord.EventType)},
                    {toSnakeCase(nameof(EvDbMessageRecord.Topic))} as {nameof(EvDbMessageRecord.Topic)},
                    {toSnakeCase(nameof(EvDbMessageRecord.MessageType))} as {nameof(EvDbMessageRecord.MessageType)},
                    {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))} as {nameof(EvDbMessageRecord.CapturedAt)},
                    {toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))} as {nameof(EvDbMessageRecord.CapturedBy)},
                    {toSnakeCase(nameof(EvDbMessageRecord.Payload))} as {nameof(EvDbMessageRecord.Payload)}                  
                FROM {context}topic WITH (READCOMMITTEDLOCK)
                ORDER BY {toSnakeCase(nameof(EvDbMessageRecord.Offset))};
                """;
    }

    public async IAsyncEnumerable<EvDbMessage> GetMessagesFromTopicsAsync()
    {
        await _connection.OpenAsync();
        DbDataReader reader = await _connection.ExecuteReaderAsync(_topicQuery);
        var parser = reader.GetRowParser<EvDbMessageRecord>();
        while (await reader.ReadAsync())
        {
            EvDbMessage e = parser(reader);
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
