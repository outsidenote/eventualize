using System.Data.Common;

namespace EvDb.Core.Adapters;

public abstract class EvDbConnectionFactory : IEvDbConnectionFactory
{
    public abstract DbConnection CreateConnection();
}
