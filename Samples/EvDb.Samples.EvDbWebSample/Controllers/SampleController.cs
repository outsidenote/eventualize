using Microsoft.AspNetCore.Mvc;

namespace EvDb.Samples.EvDbWebSample.Controllers;
[ApiController]
[Route("[controller]")]
public class SampleController : ControllerBase
{
    private readonly ILogger<SampleController> _logger;

    public SampleController(ILogger<SampleController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public Task<string> GetAsync()
    {
        throw new NotImplementedException();
    }
}
