namespace EvDb.Core;

#pragma warning disable S2326 // Unused type parameters should be removed
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbAttachTopicTablesAttribute<T> : Attribute
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
