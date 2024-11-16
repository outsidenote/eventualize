namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true)]
public class EvDbDefineMessagePayloadAttribute : EvDbDefineEventPayloadAttribute
{
    public EvDbDefineMessagePayloadAttribute(
        string payloadType) : base(payloadType) { }
}
