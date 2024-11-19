using EvDb.Core;

namespace EvDb.StressTests.Views.MinInterval;

[EvDbViewType<double, IEvents>("minimal-interval")]
internal partial class View
{
    private DateTimeOffset? _lastEventTime;

    protected override double DefaultState => -1;
    public override int MinEventsBetweenSnapshots => 5;

    #region override ...

    protected override double Fold(double state, FaultOccurred payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Fold(double state, SomethingHappened payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    #endregion // override ...

    private double CalcInterval(ref double state, IEvDbEventMeta meta)
    {
        var now = TimeProvider.GetUtcNow();
        if (_lastEventTime != null)
        {
            state = (now - _lastEventTime).Value.TotalSeconds;
        }
        _lastEventTime = meta.CapturedAt;
        return state;
    }


}
