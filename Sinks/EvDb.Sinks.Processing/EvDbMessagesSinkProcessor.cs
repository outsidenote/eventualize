using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EvDb.Sinks.Processing;

internal class EvDbMessagesSinkProcessor : IEvDbMessagesSinkProcessor
{
    private readonly ILogger _logger;
    private readonly IEvDbChangeStream _changeStream;
    private readonly IEnumerable<IEvDbTargetedMessagesSinkPublish> _sinkProvider;
    private readonly SinkBag _bag;

    public EvDbMessagesSinkProcessor(ILogger<EvDbMessagesSinkProcessor> logger,
                                     IEvDbChangeStream changeStream,
                                     IEnumerable<IEvDbTargetedMessagesSinkPublish> sinkProvider,
                                     SinkBag bag)
    {
        _logger = logger;
        _changeStream = changeStream;
        _sinkProvider = sinkProvider;
        _bag = bag;
    }

    async Task IEvDbMessagesSinkProcessor.StartMessagesSinkAsync(CancellationToken cancellationToken)
    {
        #region Validation

        if(!_sinkProvider.Any())
        {
            _logger.LogSinkInMissing(_bag.Id);
            return;
        }

        #endregion //  Validation

        _logger.LogStartListening(_bag);
        var messages = _changeStream.GetMessageRecordsAsync(_bag.Shard, _bag.Filter, _bag.Options, cancellationToken);
        await foreach (EvDbMessageRecord message in messages)
        {
            await Task.WhenAll(_sinkProvider.Select(p => p.PublishMessageToSinkAsync(message, cancellationToken)));
        }
    }
}
