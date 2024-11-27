using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Core;

[Flags]
public enum StorageFeatures
{ 
    None,
    Stream = 1,
    Snapshot = Stream * 2,
    Outbox = Snapshot * 2,
    StreamAndOutbox = Stream | Outbox,
    All = Stream | Snapshot | Outbox
}
