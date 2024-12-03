// Ignore Spelling: Occ

using System.Data.Common;

namespace EvDb.Core.Adapters;

public interface IEvDbRecordParserFactory
{
    IEvDbRecordParser Create(DbDataReader reader);
}
