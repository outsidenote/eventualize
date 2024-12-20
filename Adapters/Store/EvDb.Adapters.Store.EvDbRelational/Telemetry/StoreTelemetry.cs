﻿using System.Diagnostics;

namespace EvDb.Core.Adapters;

internal static class StoreTelemetry
{
    public const string TraceName = "EvDb:Store";

    public static ActivitySource Trace { get; } = new ActivitySource(TraceName);

    public static IEvDbStoreMeters StoreMeters { get; } = new EvDbStoreMeters();
}

