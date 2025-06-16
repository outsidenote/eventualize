using Microsoft.Extensions.Hosting;

namespace EvDb.Sinks.Processing;

internal class EvDbSinkProcessorHost : BackgroundService
{
    private readonly IEvDbMessagesSinkProcessor _sink;

    public EvDbSinkProcessorHost(IEvDbMessagesSinkProcessor sink)
    {
        _sink = sink;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _sink.StartMessagesSinkAsync(stoppingToken);
    }
}
