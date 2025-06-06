// Ignore Spelling: Occ

namespace EvDb.Core.Adapters;

public interface IEvDbRecordParser
{
    /// <summary>
    /// Parses the event.
    /// </summary>
    /// <returns></returns>
    EvDbEventRecord ParseEvent();

    /// <summary>
    /// Parses the message.
    /// </summary>
    /// <returns></returns>
    EvDbMessageRecord ParseMessage();
}
