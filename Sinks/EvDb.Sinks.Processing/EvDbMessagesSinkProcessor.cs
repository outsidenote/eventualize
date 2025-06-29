using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EvDb.Sinks.Processing;

internal class EvDbMessagesSinkProcessor : IEvDbMessagesSinkProcessor
{
    private readonly ILogger _logger;
    private readonly IEvDbChangeStream _changeStream;
    private readonly IEnumerable<IEvDbTargetedMessagesSinkPublish> _sinkProviders;
    private readonly SinkBag _bag;

    public EvDbMessagesSinkProcessor(ILogger<EvDbMessagesSinkProcessor> logger,
                                     IEvDbChangeStream changeStream,
                                     IEnumerable<IEvDbTargetedMessagesSinkPublish> sinkProviders,
                                     SinkBag bag)
    {
        _logger = logger;
        _changeStream = changeStream;
        _sinkProviders = sinkProviders.DistinctBy(m => m.Kind);
        _bag = bag;
    }

    async Task IEvDbMessagesSinkProcessor.StartMessagesSinkAsync(CancellationToken cancellationToken)
    {
        #region Validation

        if (!_sinkProviders.Any())
        {
            _logger.LogSinkInMissing(_bag.Id);
            return;
        }

        #endregion //  Validation

        _logger.LogStartListening(_bag);
        var messages = _changeStream.GetRecordsFromOutboxAsync(_bag.Shard, _bag.Filter, _bag.Options, cancellationToken);
        await foreach (EvDbMessageRecord message in messages)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (Debugger.IsAttached)
            {
                foreach (var p in _sinkProviders)
                {
                    await p.PublishMessageToSinkAsync(message, cancellationToken);
                }
            }
            else
                await Task.WhenAll(_sinkProviders.Select(p => p.PublishMessageToSinkAsync(message, cancellationToken)));
        }
    }
}
