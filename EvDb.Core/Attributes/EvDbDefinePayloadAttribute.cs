namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true)]
public class EvDbDefinePayloadAttribute : Attribute
{
    public EvDbDefinePayloadAttribute(string payloadType)
    {
        PayloadType = payloadType;
    }

    public string PayloadType { get; }
}
