using EvDb.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Core;

public interface IEvDbStorageScripting
{
    EvDbMigrationQueryTemplates CreateScripts(
                                    EvDbStorageContext context,
                                    StorageFeatures features,
                                    IEnumerable<EvDbShardName> shardNames);
}
