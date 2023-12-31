using System.Data.Common;

namespace EvDb.Core.Adapters;

public interface IEvDbConnectionFactory
{
    DbConnection CreateConnection();
}
