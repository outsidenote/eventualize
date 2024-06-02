using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;

namespace EvDb.Samples.EvDbWebSample;

public static class OtelExtensions
{

    #region AddDefaultNetCoreTraceFilters

    public static AspNetCoreTraceInstrumentationOptions AddDefaultNetCoreTraceFilters(
               this AspNetCoreTraceInstrumentationOptions options)
    {
        options.Filter = (httpContext) =>
        {
            var path = httpContext.Request.Path;
            if (path.StartsWithSegments("/swagger"))
                return false;
            if (path.StartsWithSegments("/_vs"))
                return false;
            if (path.StartsWithSegments("/_framework"))
                return false;
            if (path == "/health" ||
                           path == "/favicon.ico" ||
                                          path == "/metrics")
            {
                return false;
            }

            return true;
        };

        return options;
    }

    #endregion // AddDefaultNetCoreTraceFilters

    #region AddDefaultHttpClientTraceFilters

    public static HttpClientTraceInstrumentationOptions AddDefaultHttpClientTraceFilters(
               this HttpClientTraceInstrumentationOptions options)
    {
        options.FilterHttpRequestMessage = (request) =>
        {
            var path = request.RequestUri?.LocalPath;
            if (path?.EndsWith("/getScriptTag") ?? false)
                return false;
            return true;
        };

        return options;
    }

    #endregion // AddDefaultHttpClientTraceFilters

}
