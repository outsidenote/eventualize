using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Sinks.EvDbSinkAbstractions;
internal interface ISinkCheckpointStore
{
    public Task<bool> TryGetCheckpointAsync(string sinkName, out long checkpoint);
}
