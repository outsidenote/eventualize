using EvDb.Scenes;
using EvDb.UnitTests;
using Microsoft.AspNetCore.Mvc;

namespace EvDb.Samples.EvDbWebSample.Controllers;
[ApiController]
[Route("[controller]")]
public class SampleController : ControllerBase
{
    private readonly ILogger<SampleController> _logger;
    private readonly IEvDbIssueStreamFactory _factory;

    public SampleController(
        ILogger<SampleController> logger,
        IEvDbIssueStreamFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    [HttpGet]
    public async Task<CourseCreatedEvent[]> GetAsync()
    {
        IProblems stream = await _factory.GetAsync("demo");
        return stream.Views.Courses.ToArray();
    }

    [HttpPost]
    public async Task PostAsync([FromBody] int i)
    {
        IProblems stream = await _factory.GetAsync("demo");
        var e1 = new CourseCreatedEvent(i, $"name {i}", i + 10);
        await stream.AppendAsync(e1);
        var e2 = new ScheduleTestEvent(i * 100, new TestEntity(i * 30 + 4, "bla bla", DateTimeOffset.Now.AddDays(i)));
        await stream.AppendAsync(e2);
        await stream.StoreAsync();
    }
}
