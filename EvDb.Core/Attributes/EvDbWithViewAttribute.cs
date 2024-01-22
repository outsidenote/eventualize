namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbWithViewAttribute<T> : Attribute
    where T : IEvDbView
{
    public string? PropertyName { get; set; }
}