using EvDb.Core;
using EvDb.StressTests;
using EvDb.StressTestsWebApi;
using EvDb.StressTestsWebApi.Controllers;
using EvDb.StressTestsWebApi.Outbox;
using System.Threading.Channels;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddScoped<EvDbStorageContext>(_ => new EvDbTestStorageContext());
services.AddEvDbSqlServerStoreMigration(OutboxShards.Table1, OutboxShards.Table2);
services.AddEvDb()
        .AddDemoStreamFactory(c => c.UseSqlServerStoreForEvDbStream())
        .DefaultSnapshotConfiguration(c => c.UseSqlServerForEvDbSnapshot());
builder.AddOtel();
services.AddSingleton(Channel.CreateUnbounded<StressOptions>());
services.AddHostedService<StressJob>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
