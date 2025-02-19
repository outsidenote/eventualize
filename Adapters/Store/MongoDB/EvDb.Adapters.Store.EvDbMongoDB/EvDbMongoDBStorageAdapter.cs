// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;

namespace EvDb.Adapters.Store.MongoDB;

internal class EvDbMongoDBStorageAdapter: IEvDbStorageStreamAdapter,
                                          IEvDbStorageSnapshotAdapter
{
}
