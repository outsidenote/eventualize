#pragma warning disable S2326 // Unused type parameters should be removed

namespace EvDb.Core;

/// <summary>
/// Take ownership of the serialization at the transformation from specialized message into byte[]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbAddOutboxSerializationAttribute<TSerializer> : Attribute
    where TSerializer : IEvDbOutboxSerializer
{
    public EvDbAddOutboxSerializationAttribute(EvDbOutboxSerializationMode mode = EvDbOutboxSerializationMode.Strict)
    {

    }
}

/// <summary>
/// Take ownership of the serialization at the transformation from specialized message into byte[]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbAddOutboxSerializationAttribute<TSerializer1, TSerializer2> : Attribute
    where TSerializer1 : IEvDbOutboxSerializer
    where TSerializer2 : IEvDbOutboxSerializer
{
    public EvDbAddOutboxSerializationAttribute(EvDbOutboxSerializationMode mode = EvDbOutboxSerializationMode.Strict)
    {

    }
}
