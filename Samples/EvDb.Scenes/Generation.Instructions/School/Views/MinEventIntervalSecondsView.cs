using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbViewType<double, IEvDbSchoolStreamAdders>("min-interval-seconds")]
public partial class MinEventIntervalSecondsView
{
    private DateTimeOffset? _lastEventTime;

    protected override double DefaultState => -1;
    public override bool ShouldStoreSnapshot(long offsetGapFromLastSave, TimeSpan durationSinceLastSave) => offsetGapFromLastSave > 5;

    #region override ...

    protected override double Apply(double state, CourseCreatedEvent payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Apply(double state, ScheduleTestEvent payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Apply(double state, StudentAppliedToCourseEvent payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Apply(double state, StudentCourseApplicationDeniedEvent payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Apply(double state, StudentEnlistedEvent payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Apply(double state, StudentQuitCourseEvent payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Apply(double state, StudentReceivedGradeEvent payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Apply(double state, StudentRegisteredToCourseEvent payload, IEvDbEventMeta meta)
    {
        return CalcInterval(ref state, meta);
    }

    protected override double Apply(double state, StudentTestSubmittedEvent payload, IEvDbEventMeta meta)
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