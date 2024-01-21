namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public class EvDbEventTypeAttribute<T> : Attribute
    where T : IEvDbEventPayload
{
}