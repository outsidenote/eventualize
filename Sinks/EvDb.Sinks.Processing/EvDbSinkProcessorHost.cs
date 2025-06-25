using Microsoft.Extensions.Hosting;

namespace EvDb.Sinks.Processing;

internal class EvDbSinkProcessorHost : BackgroundService
{
    private readonly IEvDbMessagesSinkProcessor _sink;
    private readonly Task _isReady;

    /// <summary>
    /// Creates a new instance of the EvDbSinkProcessorHost.
    /// </summary>
    /// <param name="sink">The processor</param>
    /// <param name="isReady">Enable to postpone the hosted service execution until readiness of the environment</param>
    public EvDbSinkProcessorHost(IEvDbMessagesSinkProcessor sink, Task? isReady = null)
    {
        _sink = sink;
        _isReady = isReady ?? Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _isReady;
        await _sink.StartMessagesSinkAsync(stoppingToken);
    }
}
