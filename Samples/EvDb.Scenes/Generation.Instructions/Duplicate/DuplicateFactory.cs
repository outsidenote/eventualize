using EvDb.Core;

namespace EvDb.UnitTests;

[EvDbAttachView<DuplicateView>]
[EvDbStreamFactory<IEvDbSchoolStreamAdders, EvDbDuplicateOutbox>(
                        "duplicate", "issue")]
// Lifetime = ServiceLifetime.Scoped)]
public partial class DuplicateFactory
{
}

