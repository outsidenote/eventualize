namespace EvDb.Core;

public interface IEvDbView
{
    /// <summary>
    /// Gets the offset of the last folded event (in-memory).
    /// </summary>
    long MemoryOffset { get; }

    /// <summary>
    /// Gets the name of the view.
    /// </summary>
    EvDbViewAddress Address { get; }

    /// <summary>
    /// The offset of the last snapshot that was stored.
    /// </summary>
    long StoreOffset { get; }
}
