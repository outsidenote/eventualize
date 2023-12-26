using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core;
public record EventualizeeSnapshotRelationalRecrod(string SerializedState, string Domain, string StreamType, string StreamId, string AggregateType, long Offset)
    : EventualizeSnapshotCursor(Domain, StreamType, StreamId, AggregateType, Offset)
{ }