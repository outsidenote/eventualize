
using EvDb.Samples.EvDbWebSample.Controllers;

namespace EvDb.Samples.EvDbWebSample;

internal class SampleJob : IHostedService
{
    private readonly ILogger<SampleJob> _logger;

    public SampleJob(ILogger<SampleJob> logger)
    {
        _logger = logger;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
