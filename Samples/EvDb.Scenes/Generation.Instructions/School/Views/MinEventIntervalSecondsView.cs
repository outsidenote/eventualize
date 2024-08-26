using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbViewType<double, IEvDbSchoolStreamAdders>("min-interval-seconds")]
public partial class MinEventIntervalSecondsView
{
    private DateTimeOffset? _lastEventTime;

    protected override double DefaultState => -1;
    public override int MinEventsBetweenSnapshots => 5;

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