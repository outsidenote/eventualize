using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbPublicationTypes
{
    void OnPublication(
            EvDbEvent e,
            IImmutableList<IEvDbViewStore> views);
}
