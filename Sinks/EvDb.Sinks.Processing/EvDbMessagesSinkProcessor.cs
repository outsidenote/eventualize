using EvDb.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EvDb.Sinks.Processing;
internal class EvDbMessagesSinkProcessor : IEvDbMessagesSinkProcessor
{
    private readonly ILogger _logger;
    private readonly IEvDbChangeStream _changeStream;
    private readonly SinkBag _bag;

    public EvDbMessagesSinkProcessor(ILogger<EvDbMessagesSinkProcessor> logger,
                                     IEvDbTargetedMessagesSinkPublish sinkProvider,
                                     IEvDbChangeStream changeStream,
                                     SinkBag bag)
    {
        _logger = logger;
        _changeStream = changeStream;
        _bag = bag;
    }

    async Task IEvDbMessagesSinkProcessor.StartMessagesSinkAsync(CancellationToken cancellationToken)
    {
        var messages = _changeStream.GetMessagesAsync(_bag.Shard, _bag.Filter, _bag.Options, cancellationToken);
        await foreach (var message in messages)
        {

        }
        // iterate over messages
        throw new NotImplementedException();
    }
}
