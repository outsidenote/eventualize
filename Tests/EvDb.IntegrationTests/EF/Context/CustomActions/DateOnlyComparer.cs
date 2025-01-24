namespace EvDb.IntegrationTests.EF;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public class DateOnlyComparer : ValueComparer<DateOnly>
{
    public DateOnlyComparer() : base(
        (d1, d2) => d1 == d2,
        d => d.GetHashCode())
    { }
}
