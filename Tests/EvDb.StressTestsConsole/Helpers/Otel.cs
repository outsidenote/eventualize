using Cocona.Builder;
using EvDb.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EvDb.StressTests;

internal static class OtelExtensions
{
    private const string APP_NAME = "evdb:stress";
    public static CoconaAppBuilder AddOtel(this CoconaAppBuilder builder)
    {
        #region Logging

        ILoggingBuilder loggingBuilder = builder.Logging;
        loggingBuilder.AddOpenTelemetry(logging =>
        {
            var resource = ResourceBuilder.CreateDefault();
            logging.SetResourceBuilder(resource.AddService(
                            APP_NAME));  // builder.Environment.ApplicationName
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

        #endregion // Logging}

        var services = builder.Services;
        services.AddOpenTelemetry()
                    .ConfigureResource(resource =>
                                   resource.AddService(APP_NAME,
                                                    serviceInstanceId: "console-app",
                                                    autoGenerateServiceInstanceId: false)) // builder.Environment.ApplicationName
            .WithTracing(tracing =>
            {
                tracing
                        .AddEvDbInstrumentation()
                        .AddEvDbStoreInstrumentation()
                        .AddSqlClientInstrumentation(o =>
                        {
                            o.SetDbStatementForText = true;
                            o.SetDbStatementForStoredProcedure = true;
                        })
                        .SetSampler<AlwaysOnSampler>()
                        .AddOtlpExporter()
                        //.AddOtlpExporter("jaeger", o => o.Endpoint = new Uri("http://localhost:4327/"))
                        //.AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345/"))
                        .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889"))
                        ;
            })
            .WithMetrics(meterBuilder =>
                    meterBuilder.AddEvDbInstrumentation()
                                .AddEvDbStoreInstrumentation()
                                .AddProcessInstrumentation()
                                //.AddOtlpExporter()
                                //.AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345"))
                                .AddOtlpExporter("grafana", o => o.Endpoint = new Uri($"http://localhost:4337"))
                                .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889"))
            );

        return builder;
    }

}
