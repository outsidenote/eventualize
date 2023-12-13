namespace Eventualize.Core.Adapters.SqlStore;

// TODO: [bnaya 2023-12-10] review responsibility, abstraction and implementation (testcontextid_ shouldn't be part of this class in this place)
// TODO: [bnaya 2023-12-10] consider builder patten to setup the context
// TODO: [bnaya 2023-12-10] consider to put the context into the DI
internal static class SqlOperations
{
    public static string GetCreateEnvironmentQuery(StorageContext contextId)
    {
        return CreateEnvironmentQuery.GetSqlString(contextId);
    }

    public static string GetDestroyEnvironmentQuery(StorageContext contextId)
    {
        return DestroyEnvironmentQuery.GetSqlString(contextId);
    }
}