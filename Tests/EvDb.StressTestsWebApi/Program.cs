using EvDb.StressTests;
using EvDb.StressTestsWebApi;

var context = new EvDbTestStorageContext();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddEvDbDemoStreamFactory();
services.AddEvDbSqlServerStoreMigration(context);
services.AddEvDbSqlServerStore(context);
builder.AddOtel();

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
