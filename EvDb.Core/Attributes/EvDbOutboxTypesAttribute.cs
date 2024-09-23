#pragma warning disable S2326 // Unused type parameters should be removed
namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EvDbMessageTypesAttribute<T> : Attribute
    where T : IEvDbPayload
{
}


