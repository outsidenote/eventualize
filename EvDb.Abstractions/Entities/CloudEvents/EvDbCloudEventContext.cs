namespace EvDb.Core;

/// <summary>
/// Cloud Event's additional attributes
/// </summary>
/// <param name="Source">
/// Mapped to `schema` attribute.
/// Identifies the context in which an event happened.
/// Often this will include information such as the type of the event source,
/// the organization publishing the event or the process that produced the event. 
/// The exact syntax and semantics behind the data encoded in the URI is defined
/// by the event producer.
/// </param>
/// <remarks>
/// [Cloud Events](https://cloudevents.io/)
/// [Cloud Events Attributes](https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md#required-attributes)
/// [Http Cloud Events](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/http-protocol-binding.md)
/// [Kafka Cloud Events](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/kafka-protocol-binding.md)
/// </remarks>
public record EvDbCloudEventContext(Uri Source)
{
    /// <summary>
    /// Mapped to `specversion`
    /// </summary>
    public const string SpecVersion = "1.0";

    /// <summary>
    /// Cloud event's data schema URI
    /// Mapped to `dataschema` attribute.
    /// Identifies the schema that data adheres to. 
    /// Incompatible changes to the schema SHOULD be reflected by a different URI. 
    /// </summary>
    public Uri? DataSchemaUri { get; init; }
}
