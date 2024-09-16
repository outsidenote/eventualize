namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true)]
public class EvDbPayloadAttribute : Attribute
{
    public EvDbPayloadAttribute(string payloadType)
    {
        PayloadType = payloadType;
    }

    public string PayloadType { get; }
}
