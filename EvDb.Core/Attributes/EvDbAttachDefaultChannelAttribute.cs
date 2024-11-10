#pragma warning disable S2326 // Unused type parameters should be removed
namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class EvDbAttachDefaultChannelAttribute : EvDbAttachChannelAttribute
{
    public EvDbAttachDefaultChannelAttribute() : base(EvDbOutbox.DEFAULT_OUTBOX)
    {
    }
}