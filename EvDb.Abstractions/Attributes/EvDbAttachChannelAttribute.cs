#pragma warning disable S2326 // Unused type parameters should be removed
namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class EvDbAttachChannelAttribute : Attribute
{
    public EvDbAttachChannelAttribute(string channel)
    {
        Channel = channel;
    }
    public string? Channel { get; }
}
