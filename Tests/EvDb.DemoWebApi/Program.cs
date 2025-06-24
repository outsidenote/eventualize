using EvDb.Core;
using EvDb.Demo;
using EvDb.DemoWebApi;
using EvDb.DemoWebApi.Controllers;
using EvDb.DemoWebApi.Outbox;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Channels;
using static EvDb.DemoWebApi.DemoConstants;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddScoped<EvDbStorageContext>(_ => new EvDbTestStorageContext());
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

// Sink
services.AddEvDb()
        .AddSink()
        .ForMessages()
            .AddFilter(EvDbMessageFilter.Create(DateTimeOffset.UtcNow.AddSeconds(-2))
                                        .AddChannel(CommentsMessage.Channels.Comments))
            .AddOptions(EvDbContinuousFetchOptions.ContinueWhenEmpty)
            .BuildHostedService() 
            .SendToSNS(TOPIC_NAME);

services.AddSingleton<State>();


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
