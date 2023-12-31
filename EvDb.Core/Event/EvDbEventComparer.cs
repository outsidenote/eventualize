namespace EvDb.Core;

public class EvDbEventComparer : IEqualityComparer<IEvDbEvent>
{
    public static IEqualityComparer<IEvDbEvent> Default { get; } = new EvDbEventComparer();

    private EvDbEventComparer()
    {
    }

    bool IEqualityComparer<IEvDbEvent>.Equals(IEvDbEvent? x, IEvDbEvent? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.EventType == y.EventType &&
            x.CapturedBy == y.CapturedBy &&
            ((EvDbEvent)x).Data == ((EvDbEvent)y).Data;
    }

    int IEqualityComparer<IEvDbEvent>.GetHashCode(IEvDbEvent obj)
    {
        return obj.EventType.GetHashCode() ^
               obj.CapturedBy.GetHashCode() ^
               ((EvDbEvent)obj).Data.GetHashCode();
    }
}
