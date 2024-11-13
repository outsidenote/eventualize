using EvDb.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.UnitTests;

// TODO: [bnaya 2024-11-07] Consider how to expose it as a const (for pattern matching keys)
[EvDbOutboxChannels]
public abstract class OutboxChannels
{
    public const string Channel1 = "channel-1";
    public const string Channel2 = "channel-2";
    public const string Channel3 = "channel-3";
}