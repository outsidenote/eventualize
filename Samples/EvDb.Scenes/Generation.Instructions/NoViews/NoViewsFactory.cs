using EvDb.Core;

namespace EvDb.UnitTests;

[EvDbStreamFactory<IEvDbSchoolStreamAdders, EvDbNoViewsOutbox>(
                        "school-records1:students")]
public partial class NoViewsFactory
{
}

