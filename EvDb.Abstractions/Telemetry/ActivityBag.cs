using System.Diagnostics;

namespace EvDb.Core;

/// <summary>
/// Useful as enumerable result in order to maintain the current activity as the one exist on the yield return context.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Activity"></param>
/// <param name="Value"></param>
public readonly record struct ActivityBag<T>(Activity? Activity, T Value)
{
    public static implicit operator Activity?(ActivityBag<T> source) => source.Activity;
    public static implicit operator T?(ActivityBag<T> source) => source.Value;

    public void SetAsCurrentActivity()
    {
        if (Activity is not null)
        {
            Activity.Current = Activity;
        }
    }
}