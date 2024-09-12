using EvDb.Core;
using EvDb.Samples.EvDbWebSample;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Publish services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var services = builder.Services;
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
var evdb = services.AddEvDb();
evdb.AddSchoolStreamFactory(c => c.UseSqlServerStoreForEvDbStream())
    .DefaultSnapshotConfiguration(c => c.UseSqlServerForEvDbSnapshot());
evdb.AddIssueStreamFactory(c => c.UseSqlServerStoreForEvDbStream())
    .DefaultSnapshotConfiguration(c => c.UseSqlServerForEvDbSnapshot());

#region services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer)

var redisConnStr = builder.Configuration.GetConnectionString("RedisCache")
                                ?? Environment.GetEnvironmentVariable("REDIS_CACHE")
                                ?? "localhost:6379";
IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnStr);
services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

#endregion // services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer)

#region services.AddStackExchangeRedisCache(...)

// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-8.0
services.AddStackExchangeRedisCache(options =>
{
    options.ConnectionMultiplexerFactory =
            () => Task.FromResult(connectionMultiplexer);
    options.InstanceName = "DemoInstance";
});

#endregion // services.AddStackExchangeRedisCache(...)

#region Logging

ILoggingBuilder loggingBuilder = builder.Logging;
loggingBuilder.AddOpenTelemetry(logging =>
{
    var resource = ResourceBuilder.CreateDefault();
    logging.SetResourceBuilder(resource.AddService(
                    builder.Environment.ApplicationName));
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter()
           .AddOtlpExporter("jaeger", o => o.Endpoint = new Uri("http://localhost:4327/"))
           .AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345/"))
           .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889"));
});

loggingBuilder.Configure(x =>
{
    x.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
      | ActivityTrackingOptions.TraceId
      | ActivityTrackingOptions.ParentId
      | ActivityTrackingOptions.Tags;
    // | ActivityTrackingOptions.TraceState;
});

#endregion // Logging

services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                           resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing =>
    {
        tracing
                .AddEvDbInstrumentation()
                .AddRedisInstrumentation(connectionMultiplexer)
                .AddSqlClientInstrumentation(o =>
                {
                    o.SetDbStatementForText = true;
                    o.SetDbStatementForStoredProcedure = true;
                    o.EnableConnectionLevelAttributes = true;
                    o.RecordException = true;
                })
                .AddAspNetCoreInstrumentation(o => o.AddDefaultNetCoreTraceFilters())
                .AddHttpClientInstrumentation(o => o.AddDefaultHttpClientTraceFilters())
                .SetSampler<AlwaysOnSampler>()
                //.SetSampler(new AlwaysOnSampler())
                .AddOtlpExporter()
                .AddOtlpExporter("jaeger", o => o.Endpoint = new Uri("http://localhost:4327/"))
                .AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345/"))
                .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889"));
    })
    .WithMetrics(meterBuilder =>
            meterBuilder.AddEvDbInstrumentation()
                        .AddProcessInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddPrometheusExporter()
                        .AddOtlpExporter()
                        .AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345"))
                        .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889")));




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

app.Run();

