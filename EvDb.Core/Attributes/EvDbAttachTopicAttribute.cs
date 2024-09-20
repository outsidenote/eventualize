#pragma warning disable S2326 // Unused type parameters should be removed
namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class EvDbAttachTopicAttribute : Attribute
{
    public EvDbAttachTopicAttribute(string topic)
    {
        Topic = topic;
    }
    public string? Topic { get; }
}