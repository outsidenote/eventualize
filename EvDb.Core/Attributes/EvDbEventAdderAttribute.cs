namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public class EvDbEventAdderAttribute<T> : Attribute
    where T : IEvDbEventPayload
{
}