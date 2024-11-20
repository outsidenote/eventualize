using EvDb.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.MinimalStructure;

[EvDbDefineEventPayload("sample")]
public readonly partial record struct EventSample(int Value)
{
}
