namespace EvDb.Core;

public class EvDbEventComparer : IEqualityComparer<EvDbEvent>
{
    public static IEqualityComparer<EvDbEvent> Default { get; } = new EvDbEventComparer();

    private EvDbEventComparer()
    {
    }

    bool IEqualityComparer<EvDbEvent>.Equals(EvDbEvent x, EvDbEvent y)
    {
        return x.EventType == y.EventType &&
            x.CapturedBy == y.CapturedBy &&
            x.Payload == y.Payload;
    }

    int IEqualityComparer<EvDbEvent>.GetHashCode(EvDbEvent obj)
    {
        return obj.EventType.GetHashCode() ^
               obj.CapturedBy.GetHashCode() ^
               ((EvDbEvent)obj).Payload.GetHashCode();
    }
}
