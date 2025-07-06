using Confluent.Kafka;
using EvDb.Core;
using EvDb.Demo;
using EvDb.DemoWebApi;
using EvDb.DemoWebApi.Outbox;
using System.Threading.Channels;
using static EvDb.DemoWebApi.DemoConstants;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddSingleton<EvDbStorageContext>(_ => EvDbStorageContext.CreateWithEnvironment("master", "demo", schema: "dbo"));
services.AddEvDbSqlServerStoreAdmin();
services.AddEvDb()
        .AddDemoStreamFactory(c => c.UseSqlServerStoreForEvDbStream())
        .DefaultSnapshotConfiguration(c => c.UseSqlServerForEvDbSnapshot());
builder.AddOtel();
services.AddSingleton(Channel.CreateUnbounded<DemoOptions>());


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Change Stream
services.UseSqlServerChangeStream();

// Sink
services.AddSingleton(AWSProviderFactory.CreateSQSClient());
services.AddSingleton(AWSProviderFactory.CreateSNSClient());


#region Kafka

services.AddSingleton<IProducer<string, string>>(sp =>
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        var producer = new ProducerBuilder<string, string>(config)
            //.SetValueSerializer(new EvDbMessageSerializer())
            .Build();
        return producer;
    });

#endregion //  Kafka

services.AddEvDb()
        .AddSink()
        .ForMessages()
            //.AddShard()
            .AddFilter(EvDbMessageFilter.Create(DateTimeOffset.UtcNow.AddSeconds(-2))
                                        .AddChannel(CommentsMessage.Channels.Comments))
            .AddOptions(EvDbContinuousFetchOptions.ContinueWhenEmpty)
            .BuildHostedService(CreateEnvironmentAsync)
            .SendToSNS(TOPIC_NAME)
            .SendToKafka(TOPIC_NAME);

services.AddSingleton<State>();
services.AddHostedService<SinkJob>();
services.AddHostedService<DataProducerJob>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

#region CreateEnvironmentAsync

async Task CreateEnvironmentAsync(IServiceProvider sp, CancellationToken cancellationToken)
{
    try
    {
        var admin = sp.GetRequiredService<IEvDbStorageAdmin>();
        await admin.CreateEnvironmentAsync(cancellationToken);
    }
    catch (Microsoft.Data.SqlClient.SqlException)
    {
        // Assume already exists
    }
}

#endregion //  CreateEnvironmentAsync
