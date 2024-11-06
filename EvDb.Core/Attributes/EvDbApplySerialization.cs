#pragma warning disable S2326 // Unused type parameters should be removed

namespace EvDb.Core;

/// <summary>
/// Take ownership of the serialization at the transformation from specialized message into byte[]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EvDbApplyOutboxSerializationAttribute<TSerializer> : Attribute
    where TSerializer : IEvDbOutboxSerializer
{
}
