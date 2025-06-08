// Ignore Spelling: Occ

using System.Data.Common;

namespace EvDb.Core.Adapters;

public interface IEvDbRecordParserFactory
{
    IEvDbRecordParser CreateParser(DbDataReader reader);
}
