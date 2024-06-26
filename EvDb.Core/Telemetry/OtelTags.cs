﻿using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace EvDb.Core;

/// <summary>
/// Tag addition
/// </summary>
public readonly record struct OtelTags : IEnumerable<KeyValuePair<string, object?>>
{
    public OtelTags()
    {
        Tags = ImmutableArray<KeyValuePair<string, object?>>.Empty;
    }

    [Required]
    private ImmutableArray<KeyValuePair<string, object?>> Tags { get; init; }

    public static OtelTags Empty { get; } = new OtelTags();

    public OtelTags Add<T>(string key, T value)
    {
        var kv = new KeyValuePair<string, object?>(key, value);
        var tags = Tags.Add(kv);
        return this with { Tags = tags };
    }

    public static implicit operator TagList(OtelTags tags)
    {
        return new TagList(tags.Tags.AsSpan());
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        foreach (var item in Tags)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}