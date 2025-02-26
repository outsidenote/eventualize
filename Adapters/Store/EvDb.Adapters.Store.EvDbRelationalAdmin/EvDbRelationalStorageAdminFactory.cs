// Ignore Spelling: Admin

using Microsoft.Extensions.Logging;

namespace EvDb.Core.Adapters;

public class EvDbRelationalStorageAdminFactory
{
    private readonly ILogger _logger;
    private readonly IEvDbConnectionFactory _factory;
    private readonly IEvDbStorageScripting _scripting;

    public EvDbRelationalStorageAdminFactory(
        ILogger logger,
        IEvDbConnectionFactory factory,
        IEvDbStorageScripting scripting)
    {
        _logger = logger;
        _factory = factory;
        _scripting = scripting;
    }

    public IEvDbStorageAdmin Create(
        EvDbStorageContext context,
        StorageFeatures features,
        params EvDbShardName[] shardNames)
    {
        EvDbMigrationQueryTemplates queries =
                        _scripting.CreateScripts(context, features, shardNames);

        IEvDbStorageAdmin result =
            new EvDbRelationalStorageAdmin(
                    _logger,
                    _factory,
                    queries);
        return result;
    }
}
