#pragma warning disable S1133 // Deprecated code should be removed

namespace EvDb.Core;

/// <summary>
/// Optimistic Concurrency Collisions
/// </summary>
public class OCCException : Exception
{
    public OCCException() { }
    [Obsolete("Shouldn't be used directly, used by the serialization", true)]
    public OCCException(string message) : base(message) { }
 
    public OCCException(EvDbStreamCursor cursor) : base(cursor.ToString())
    {
    }
}