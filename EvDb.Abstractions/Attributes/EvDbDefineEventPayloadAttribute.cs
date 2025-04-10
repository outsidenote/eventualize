namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true)]
public class EvDbDefineEventPayloadAttribute : Attribute
{
    public EvDbDefineEventPayloadAttribute(string payloadType)
    {
        PayloadType = payloadType;
    }

    public string PayloadType { get; }
}
