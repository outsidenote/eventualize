using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace EvDb.StressTestsWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class StressController : ControllerBase
{
    private readonly ILogger<StressController> _logger;
    private readonly Channel<StressOptions> _channel;

    public StressController(
        ILogger<StressController> logger,
        Channel<StressOptions> channel)
    {
        _logger = logger;
        _channel = channel;
    }

    [HttpPost]
    public async Task PostAsync([FromBody] StressOptions options)
    {
        await _channel.Writer.WriteAsync(options);
        _logger.LogInformation("Schedule: {options}", options);
    }
}
