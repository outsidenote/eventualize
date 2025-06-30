#pragma warning disable S2326 // Unused type parameters should be removed

namespace EvDb.Core;

/// <summary>
/// Take ownership of the serialization at the transformation from specialized message into byte[]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbUseOutboxSerializationAttribute<TSerializer> : Attribute
    where TSerializer : IEvDbOutboxSerializer, new()
{
    public EvDbUseOutboxSerializationAttribute(EvDbMessageSerializationMode mode = EvDbMessageSerializationMode.Strict)
    {

    }
}

/// <summary>
/// Take ownership of the serialization at the transformation from specialized message into byte[]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbUseOutboxSerializationAttribute<TSerializer1, TSerializer2> : Attribute
    where TSerializer1 : IEvDbOutboxSerializer, new()
    where TSerializer2 : IEvDbOutboxSerializer, new()
{
    public EvDbUseOutboxSerializationAttribute(EvDbMessageSerializationMode mode = EvDbMessageSerializationMode.Strict)
    {

    }
}

/// <summary>
/// Take ownership of the serialization at the transformation from specialized message into byte[]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbUseOutboxSerializationAttribute<TSerializer1, TSerializer2, TSerializer3> : Attribute
    where TSerializer1 : IEvDbOutboxSerializer, new()
    where TSerializer2 : IEvDbOutboxSerializer, new()
    where TSerializer3 : IEvDbOutboxSerializer, new()
{
    public EvDbUseOutboxSerializationAttribute(EvDbMessageSerializationMode mode = EvDbMessageSerializationMode.Strict)
    {

    }
}

/// <summary>
/// Take ownership of the serialization at the transformation from specialized message into byte[]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbUseOutboxSerializationAttribute<TSerializer1, TSerializer2, TSerializer3, TSerializer4> : Attribute
    where TSerializer1 : IEvDbOutboxSerializer, new()
    where TSerializer2 : IEvDbOutboxSerializer, new()
    where TSerializer3 : IEvDbOutboxSerializer, new()
    where TSerializer4 : IEvDbOutboxSerializer, new()
{
    public EvDbUseOutboxSerializationAttribute(EvDbMessageSerializationMode mode = EvDbMessageSerializationMode.Strict)
    {

    }
}

/// <summary>
/// Take ownership of the serialization at the transformation from specialized message into byte[]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbUseOutboxSerializationAttribute<TSerializer1, TSerializer2, TSerializer3, TSerializer4, TSerializer5> : Attribute
    where TSerializer1 : IEvDbOutboxSerializer, new()
    where TSerializer2 : IEvDbOutboxSerializer, new()
    where TSerializer3 : IEvDbOutboxSerializer, new()
    where TSerializer4 : IEvDbOutboxSerializer, new()
    where TSerializer5 : IEvDbOutboxSerializer, new()
{
    public EvDbUseOutboxSerializationAttribute(EvDbMessageSerializationMode mode = EvDbMessageSerializationMode.Strict)
    {

    }
}

/// <summary>
/// Take ownership of the serialization at the transformation from specialized message into byte[]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbUseOutboxSerializationAttribute<TSerializer1, TSerializer2, TSerializer3, TSerializer4, TSerializer5, TSerializer6> : Attribute
    where TSerializer1 : IEvDbOutboxSerializer, new()
    where TSerializer2 : IEvDbOutboxSerializer, new()
    where TSerializer3 : IEvDbOutboxSerializer, new()
    where TSerializer4 : IEvDbOutboxSerializer, new()
    where TSerializer5 : IEvDbOutboxSerializer, new()
    where TSerializer6 : IEvDbOutboxSerializer, new()
{
    public EvDbUseOutboxSerializationAttribute(EvDbMessageSerializationMode mode = EvDbMessageSerializationMode.Strict)
    {

    }
}
