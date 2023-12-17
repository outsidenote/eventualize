using System.Data.Common;

namespace Eventualize.Core.Adapters;

public interface IEventualizeConnectionFactory
{
    DbConnection CreateConnection();
}
