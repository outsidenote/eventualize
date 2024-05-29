using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EvDb.Core;

public static class MeterExtensions
{
    public static void Add<T>(this Counter<T> counter, T count, Func<OtelTags, OtelTags> action)
        where T : struct
    {
        TagList tags = action.Invoke(OtelTags.Empty);
        counter.Add(count, in tags);
    }
}
