namespace EvDb.Core;

public enum OtelParentRelation
{
    /// <summary>
    /// Direct child of the parent span.
    /// </summary>
    Child,
    /// <summary>
    /// Parent span is used as a link to the current span.
    /// Suitable for cases when the parent span is not directly related to the current span, but still provides context.
    /// </summary>
    Link,
}

