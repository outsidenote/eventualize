using EvDb.Core;
using EvDb.Core.Internals;

namespace Microsoft.Extensions.DependencyInjection;

public static class CoreDependencyInjectionExtensions
{
    /// <summary>
    /// Serve as an anchor for extension method of the storage registration 
    /// </summary>
    /// <param name="services"></param>
    public static EvDbRegistrationEntry AddEvDb(this IServiceCollection services)
    {
        return services.AddEvDb(null);
    }

    /// <summary>
    /// Serve as an anchor for extension method of the storage registration 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="cloudEventSource">
    /// Identifies the context in which an event happened.
    /// Often this will include information such as the type of the event source,
    /// the organization publishing the event or the process that produced the event. 
    /// The exact syntax and semantics behind the data encoded in the URI is defined
    /// by the event producer.
    /// </param>
    /// <param name="cloudEventDataSchemaUri">
    /// Identifies the schema that data adheres to. 
    /// Incompatible changes to the schema SHOULD be reflected by a different URI. 
    /// </param>
    public static EvDbRegistrationEntry AddEvDb(this IServiceCollection services,
                                                string cloudEventSource,
                                                string? cloudEventDataSchemaUri = null)
    {
        if (!Uri.TryCreate(cloudEventSource, UriKind.Absolute, out Uri srcUri))
            throw new ArgumentException($"`{nameof(cloudEventSource)}` should be a Uri");
        if(string.IsNullOrEmpty(cloudEventDataSchemaUri))
            return AddEvDb(services, new EvDbCloudEventContext(srcUri));
        if (!Uri.TryCreate(cloudEventDataSchemaUri, UriKind.Absolute, out Uri scmUri))
            throw new ArgumentException($"`{nameof(cloudEventDataSchemaUri)}` should be a Uri");
        return AddEvDb(services, new EvDbCloudEventContext(srcUri) { DataSchemaUri = scmUri });
    }

    /// <summary>
    /// Serve as an anchor for extension method of the storage registration 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="cloudEventSource">
    /// Identifies the context in which an event happened.
    /// Often this will include information such as the type of the event source,
    /// the organization publishing the event or the process that produced the event. 
    /// The exact syntax and semantics behind the data encoded in the URI is defined
    /// by the event producer.
    /// </param>
    /// <param name="cloudEventDataSchemaUri">
    /// Identifies the schema that data adheres to. 
    /// Incompatible changes to the schema SHOULD be reflected by a different URI. 
    /// </param>
    public static EvDbRegistrationEntry AddEvDb(this IServiceCollection services,
                                                Uri cloudEventSource,
                                                Uri? cloudEventDataSchemaUri = null)
    {
        EvDbCloudEventContext cloudEventContext = new (cloudEventSource)
        {
            DataSchemaUri = cloudEventDataSchemaUri
        };
        return services.AddEvDb(cloudEventContext);
    }

    /// <summary>
    /// Serve as an anchor for extension method of the storage registration 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="cloudEventContext"></param>
    private static EvDbRegistrationEntry AddEvDb(this IServiceCollection services, 
                                                EvDbCloudEventContext? cloudEventContext = null)
    {
        return new EvDbRegistrationEntry(services, cloudEventContext);
    }
}
