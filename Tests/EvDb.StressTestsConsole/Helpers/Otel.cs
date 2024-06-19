using Cocona.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvDb.Core;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

namespace EvDb.StressTests;

internal static class OtelExtensions
{   
    public static CoconaAppBuilder AddOtel(this CoconaAppBuilder builder)
    {
        #region // Logging

        //ILoggingBuilder loggingBuilder = builder.Logging;
        //loggingBuilder.AddOpenTelemetry(logging =>
        //{
        //    var resource = ResourceBuilder.CreateDefault();
        //    logging.SetResourceBuilder(resource.AddService(
        //                    builder.Environment.ApplicationName));
        //    logging.IncludeFormattedMessage = true;
        //    logging.IncludeScopes = true;
        //    logging.AddOtlpExporter()
        //           .AddOtlpExporter("jaeger", o => o.Endpoint = new Uri("http://localhost:4327/"))
        //           .AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345/"))
        //           .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889"));
        //});

        //loggingBuilder.Configure(x =>
        //{
        //    x.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
        //      | ActivityTrackingOptions.TraceId
        //      | ActivityTrackingOptions.ParentId
        //      | ActivityTrackingOptions.Tags;
        //    // | ActivityTrackingOptions.TraceState;
        //});

        #endregion // Logging}

        var services = builder.Services;
        services.AddOpenTelemetry()
                    .ConfigureResource(resource =>
                                   resource.AddService("evdb:stress"))
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
                        //.AddOtlpExporter()
                        .AddOtlpExporter("jaeger", o => o.Endpoint = new Uri("http://localhost:4327/"))
                        //.AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345/"))
                        .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889"));
            })
            .WithMetrics(meterBuilder =>
                    meterBuilder.AddEvDbInstrumentation()
                                .AddProcessInstrumentation()
                                .AddHttpClientInstrumentation()
                                .AddAspNetCoreInstrumentation()
                                .AddPrometheusExporter()
                                //.AddOtlpExporter()
                                //.AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345"))
                                .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889")));
    
        return builder;
    }

}
