using System.Collections;
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
    public static OtelTags Create<T>(string key, T value) => Empty.Add(key, value);

    public OtelTags Add<T>(string key, T value)
    {
        var kv = new KeyValuePair<string, object?>(key, value);
        var tags = Tags.Add(kv);
        return this with { Tags = tags };
    }

    public OtelTags AddRange(IEnumerable<KeyValuePair<string, object?>> tags)
    {
        var result = Tags;
        foreach (var tag in tags)
        {
            result = result.Add(tag);
        }

        return this with { Tags = result };
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
