using System.Data.Common;

namespace EvDb.Core.Adapters;

public abstract class EvDbConnectionFactory : IEvDbConnectionFactory
{
    public abstract string ProviderType { get; }

    public abstract DbConnection CreateConnection();
}
