using EvDb.Core;

namespace EvDb.MinimalStructure.Views.MinInterval;

[EvDbViewType<double, IEvents>("b")]
internal partial class View
{
    private DateTimeOffset? _lastEventTime;

    protected override double DefaultState => -1;

    public override bool ShouldStoreSnapshot(long offsetGapFromLastSave, TimeSpan durationSinceLastSave) => offsetGapFromLastSave > 5;

    #region override ...

    protected override double Apply(double state, Event1 payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Apply(double state, Event2 payload, IEvDbEventMeta meta)
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
