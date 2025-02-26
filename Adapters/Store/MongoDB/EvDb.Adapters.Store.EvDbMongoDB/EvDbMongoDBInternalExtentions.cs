// Ignore Spelling: Calc

using EvDb.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Adapters.Store.EvDbMongoDB.Internals;

public static class EvDbMongoDBInternalExtentions
{
    public static string CalcCollectionPrefix(this EvDbStorageContext storageContext)
    {
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string tblInitial = $"{schema}{storageContext.ShortId}";
        return tblInitial;
    }
}
