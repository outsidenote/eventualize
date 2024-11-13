using EvDb.Core;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EvDb.StressTests;

internal static class OtelExtensions
{
    private const string APP_NAME = "evdb:stress";
    private const string OTEL_ENC_KEY = "EVDB_OTEL_EXPORTER_HOST";

    public static WebApplicationBuilder AddOtel(this WebApplicationBuilder builder)
    {
        string otelExporterServer = Environment.GetEnvironmentVariable(OTEL_ENC_KEY) ?? "localhost";
        string otelHost = $"http://{otelExporterServer}";

        #region Logging

        ILoggingBuilder loggingBuilder = builder.Logging;
        loggingBuilder.AddOpenTelemetry(logging =>
        {
            var resource = ResourceBuilder.CreateDefault();
            logging.SetResourceBuilder(resource.AddService(
                            APP_NAME));  // builder.Environment.ApplicationName
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.AddOtlpExporter(o => o.Endpoint = new Uri($"{otelHost}:4317"))
                   .AddOtlpExporter("jaeger", o => o.Endpoint = new Uri($"{otelHost}:4327/"))
                   .AddOtlpExporter("alloy", o => o.Endpoint = new Uri($"{otelHost}:12345/"))
                   .AddOtlpExporter("aspire", o => o.Endpoint = new Uri($"{otelHost}:18889"));
        });

        loggingBuilder.Configure(x =>
        {
            x.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
              | ActivityTrackingOptions.TraceId
              | ActivityTrackingOptions.Tags;
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
                        .AddSqlClientInstrumentation(o =>
                        {
                            o.SetDbStatementForText = true;
                            o.SetDbStatementForStoredProcedure = true;
                        })
                        .SetSampler<AlwaysOnSampler>()
                        .AddOtlpExporter(o => o.Endpoint = new Uri($"{otelHost}:4317"))
                        .AddOtlpExporter("grafana", o => o.Endpoint = new Uri($"{otelHost}:4337"))
                        //.AddOtlpExporter("jaeger", o => o.Endpoint = new Uri($"{otelHost}:4327/"))
                        //.AddOtlpExporter("alloy", o => o.Endpoint = new Uri($"{otelHost}:12345/"))
                        .AddOtlpExporter("aspire", o => o.Endpoint = new Uri($"{otelHost}:18889"));
            })
            .WithMetrics(meterBuilder =>
                    meterBuilder.AddEvDbInstrumentation()
                                .AddHttpClientInstrumentation()
                                .AddProcessInstrumentation()
                                .AddRuntimeInstrumentation()
                                .AddAspNetCoreInstrumentation()

                                .AddPrometheusExporter()
                                .AddOtlpExporter(o => o.Endpoint = new Uri($"{otelHost}:4317"))
                                //.AddOtlpExporter("alloy", o => o.Endpoint = new Uri($"{otelHost}:12345"))
                                .AddOtlpExporter("grafana", o => o.Endpoint = new Uri($"{otelHost}:4337"))
                                .AddOtlpExporter("aspire", o => o.Endpoint = new Uri($"{otelHost}:18889")));

        return builder;
    }

}
