namespace EvDb.Core;

public interface IEvDbViewMetadata
{
    /// <summary>
    /// Gets the offset of the last folded event.
    /// </summary>
    long FoldOffset { get; }

    int MinEventsBetweenSnapshots { get; }

    /// <summary>
    /// Gets the name of the view.
    /// </summary>
    EvDbViewAddress Address { get; }

    /// <summary>
    /// The offset of the last snapshot that was stored.
    /// </summary>
    long StoreOffset { get; set; }

    bool ShouldStoreSnapshot { get; }
}
