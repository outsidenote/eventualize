using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace EvDb.DemoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    private readonly ILogger<DemoController> _logger;
    private readonly Channel<DemoOptions> _channel;

    public DemoController(
        ILogger<DemoController> logger,
        Channel<DemoOptions> channel)
    {
        _logger = logger;
        _channel = channel;
    }

    [HttpPost]
    public async Task PostAsync([FromBody] DemoOptions options)
    {
        await _channel.Writer.WriteAsync(options);
        _logger.LogInformation("Schedule: {options}", options);
    }
}
