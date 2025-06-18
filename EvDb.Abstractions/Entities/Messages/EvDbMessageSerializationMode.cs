#pragma warning disable S2326 // Unused type parameters should be removed

namespace EvDb.Core;

public enum EvDbMessageSerializationMode
{
    /// <summary>
    /// The Permissive mode will take the first serialization that
    /// eligible for the serialization context (channel, etc.)
    /// </summary>
    Permissive,
    /// <summary>
    /// The strict requires a single serialization otherwise it will throw an exception
    /// </summary>
    Strict
}
