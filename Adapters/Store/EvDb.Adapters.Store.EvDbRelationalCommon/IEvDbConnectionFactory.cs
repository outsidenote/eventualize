using System.Data.Common;

namespace EvDb.Core.Adapters;

public interface IEvDbConnectionFactory
{
    string ProviderType { get; }
    DbConnection CreateConnection();
}
