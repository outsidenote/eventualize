#pragma warning disable S2326 // Unused type parameters should be removed
namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public class EvDbAttachEventTypeAttribute<T> : Attribute
    where T : IEvDbPayload
{
}
