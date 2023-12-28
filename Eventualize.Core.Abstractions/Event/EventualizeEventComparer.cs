namespace Eventualize.Core;

public class EventualizeEventComparer : IEqualityComparer<IEventualizeEvent>
{
    public static IEqualityComparer<IEventualizeEvent> Default { get; } = new EventualizeEventComparer();

    private EventualizeEventComparer()
    {
    }

    bool IEqualityComparer<IEventualizeEvent>.Equals(IEventualizeEvent? x, IEventualizeEvent? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        return x.EventType == y.EventType &&
            x.CapturedBy == y.CapturedBy &&
            ((EventualizeEvent)x).Data == ((EventualizeEvent)y).Data;
    }

    int IEqualityComparer<IEventualizeEvent>.GetHashCode(IEventualizeEvent obj)
    {
        return obj.EventType.GetHashCode() ^
               obj.CapturedBy.GetHashCode() ^
               ((EventualizeEvent)obj).Data.GetHashCode();
    }
}
