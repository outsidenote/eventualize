// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace EvDb.Core.Adapters;

public sealed class EvDbStoreNamingPolicy : EvDbSeparatorNamingPolicy
{
    public static readonly EvDbStoreNamingPolicy Default = new EvDbStoreNamingPolicy();

    public EvDbStoreNamingPolicy()
        : base(lowercase: true, separator: '_')
    {
    }
}

