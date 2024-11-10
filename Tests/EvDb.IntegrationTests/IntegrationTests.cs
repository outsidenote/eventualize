namespace EvDb.Core.Tests;

using Dapper;
using EvDb.Core;
using EvDb.Core.Adapters;
using EvDb.UnitTests;
using FakeItEasy;
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
        _storageMigration = StoreAdapterHelper.CreateStoreMigration(_logger, storeType, context,
                                                        OutboxShards.MessagingVip,
                                                        OutboxShards.Messaging,
                                                        OutboxShards.Commands,
                                                        EvDbShardName.Default);
        _connection = StoreAdapterHelper.GetConnection(storeType, context);
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

        // TODO: [Bnaya 2024-11-07] Add serialization name
        _outboxQuery =
            $$"""
                SELECT
                    {{toSnakeCase(nameof(EvDbMessageRecord.Domain))}} as {{nameof(EvDbMessageRecord.Domain)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.Partition))}} as {{nameof(EvDbMessageRecord.Partition)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.StreamId))}} as {{nameof(EvDbMessageRecord.StreamId)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.Offset))}} as {{nameof(EvDbMessageRecord.Offset)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.EventType))}} as {{nameof(EvDbMessageRecord.EventType)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.Channel))}} as {{nameof(EvDbMessageRecord.Channel)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.MessageType))}} as {{nameof(EvDbMessageRecord.MessageType)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))}} as {{nameof(EvDbMessageRecord.CapturedAt)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))}} as {{nameof(EvDbMessageRecord.CapturedBy)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.SpanId))}} as {{nameof(EvDbMessageRecord.SpanId)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.TraceId))}} as {{nameof(EvDbMessageRecord.TraceId)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.Payload))}} as {{nameof(EvDbMessageRecord.Payload)}}                  
                FROM {{context.Id}}{0} WITH (READCOMMITTEDLOCK)
                ORDER BY {{toSnakeCase(nameof(EvDbMessageRecord.Offset))}}, {{toSnakeCase(nameof(EvDbMessageRecord.MessageType))}};
                """;
    }

    public async IAsyncEnumerable<EvDbMessageRecord> GetMessagesFromTopicsAsync(EvDbShardName table)
    {
        await _connection.OpenAsync();
        string query = string.Format(_outboxQuery, table);
        DbDataReader reader = await _connection.ExecuteReaderAsync(query);
        var parser = reader.GetRowParser<EvDbMessageRecord>();
        while (await reader.ReadAsync())
        {
            EvDbMessageRecord e = parser(reader);
            yield return e;
        }
        await _connection.CloseAsync();
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
