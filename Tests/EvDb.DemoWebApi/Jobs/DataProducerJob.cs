using Bogus;
using EvDb.Core;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace EvDb.DemoWebApi;

public class DataProducerJob : BackgroundService
{
    private readonly ILogger<DataProducerJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEvDbStorageAdmin _admin;
    private readonly Channel<DemoOptions> _channel;
    private static readonly Faker _faker = new();
    private static readonly Random _rnd = new();

    public DataProducerJob(
        ILogger<DataProducerJob> logger,
        IServiceScopeFactory scopeFactory,
        IEvDbStorageAdmin admin,
        Channel<DemoOptions> channel)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _admin = admin;
        _channel = channel;
    }

    #region ExecuteAsync

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CreateEnvironmentAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var options = await _channel.Reader.ReadAsync(stoppingToken);
                _logger.LogInformation("Start: {options}", options);
                using (var scope = _scopeFactory.CreateScope())
                {
                    var factory = scope.ServiceProvider.GetRequiredService<IEvDbDemoStreamFactory>();
                    await RunAsync(options, factory, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
        await _admin.DestroyEnvironmentAsync(stoppingToken);
    }

    #endregion //  ExecuteAsync

    #region CreateEnvironmentAsync

    public async Task CreateEnvironmentAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await _admin.CreateEnvironmentAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to create environment");
        }
    }

    #endregion //  CreateEnvironmentAsync

    #region RunAsync

    public async Task RunAsync(
        DemoOptions options,
        IEvDbDemoStreamFactory factory,
        CancellationToken stoppingToken)
    {
        var stream = await factory.GetAsync(options.Id);
        string name = _faker.Name.FullName();
        int rate = _rnd.Next(1, 5);
        CreatedEvent created = new(name, rate);
        await stream.AppendAsync(created);

        await Task.Delay(_rnd.Next(500, 5000), stoppingToken); // Simulate some delay

        rate = _rnd.Next(1, 5);
        ModifiedEvent modified = new(rate);
        await stream.AppendAsync(modified);

        for (int i = 0; i < _rnd.Next(1, 4); i++)
        {
            await Task.Delay(_rnd.Next(500, 5000), stoppingToken); // Simulate some delay

            string commentData = _faker.Lorem.Sentence(5, 10);
            CommentedEvent comment = new(commentData);
            await stream.AppendAsync(comment);

        }
        await Task.Delay(_rnd.Next(500, 5000), stoppingToken); // Simulate some delay

        rate = _rnd.Next(1, 5);
        DeletedEvent deleted = new();
        await stream.AppendAsync(deleted);
    }

    #endregion //  RunAsync
}
