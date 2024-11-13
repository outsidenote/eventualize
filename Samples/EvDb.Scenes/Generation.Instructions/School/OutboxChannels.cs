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
    public static readonly EvDbChannelName Channel1 = "channel-1";
    public static readonly EvDbChannelName Channel2 = "channel-2";
    public static readonly EvDbChannelName Channel3 = "channel-3";
}