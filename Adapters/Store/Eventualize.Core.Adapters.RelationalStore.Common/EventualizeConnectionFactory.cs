using System.Data.Common;

namespace Eventualize.Core.Adapters;

public abstract class EventualizeConnectionFactory: IEventualizeConnectionFactory
{
    public abstract DbConnection CreateConnection();
}
