using Microsoft.AspNetCore.Mvc;

namespace EvDb.DemoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class StateController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly State _state;

    public StateController(
        ILogger<StateController> logger,
        State state)
    {
        _logger = logger;
        _state = state;
    }

    [HttpGet("id")]
    public string[] GetAsync(int id)
    {
        _logger.LogInformation("Get by id");
        if (_state.Comments.TryGetValue(id.ToString(), out var comments))
        {
            return comments.ToArray();
        }
        else
        {
            _logger.LogWarning("No comments found for id {Id}", id);
            return Array.Empty<string>();
        }
    }
}
