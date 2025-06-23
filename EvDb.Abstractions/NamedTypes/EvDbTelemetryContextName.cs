using EvDb.Core.NamedTypes.Internals;
using System.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
// ReSharper disable once UnusedType.Global

namespace EvDb.Core;

[ExcludeFromCodeCoverage]
[JsonConverter(typeof(EvDbTelemetryContextNameSystemTextJsonConverter))]
[DebuggerDisplay("{ _value }")]
public readonly partial struct EvDbTelemetryContextName :
    IEquatable<EvDbTelemetryContextName>,
    IEnumerable<byte>,
    IEvDbPayloadRawData
{
    private static readonly JsonSerializerOptions JSON_COMPARE_OPTIONS = new JsonSerializerOptions { WriteIndented = true };
    #region _stackTrace

#if DEBUG
    private readonly System.Diagnostics.StackTrace? _stackTrace = null!;
#endif

    #endregion //  _stackTrace

    private readonly byte[] _value;

    #region IEvDbPayloadRawData.RawValue

    /// <summary>
    /// Gets the underlying Byte[]
    /// </summary>
    readonly byte[] IEvDbPayloadRawData.RawValue
    {
        [System.Diagnostics.DebuggerStepThroughAttribute]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get
        {
            return _value ?? [];
        }
    }

    #endregion //  IEvDbPayloadRawData.RawValue

    #region Length

    /// <summary>
    /// Gets the length of the underlying Byte[].
    /// </summary>
    public int Length => _value?.Length ?? 0;

    #endregion //  Length

    #region Ctor

    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public EvDbTelemetryContextName()
    {
#if DEBUG
        _stackTrace = new System.Diagnostics.StackTrace();
#endif
#if !VOGEN_NO_VALIDATION
        _isInitialized = false;
#endif
        _value = [];
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbTelemetryContextName(IEnumerable<byte> value)
    {
        if (value is byte[] v)
        {
            _value = v;
        }
        else
        {
            _value = value.ToArray();
        }
#if !VOGEN_NO_VALIDATION
        _isInitialized = true;
#endif
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbTelemetryContextName(byte[] value)
    {
        _value = value;
#if !VOGEN_NO_VALIDATION
        _isInitialized = true;
#endif
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbTelemetryContextName(ReadOnlySpan<byte> value) : this(value.ToImmutableArray())
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbTelemetryContextName(Span<byte> value) : this(value.ToImmutableArray())
    {
    }

    #endregion //  Ctor

    #region Empty

    /// <summary>
    /// Represents an empty instance of this type.
    /// </summary>
    public static EvDbTelemetryContextName Empty { get; } = new EvDbTelemetryContextName(ImmutableArray<byte>.Empty);

    #endregion //  Empty

    #region From

    /// <summary>
    /// Builds an instance from byte[]. 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbTelemetryContextName FromArray(byte[] value)
    {
        return new EvDbTelemetryContextName(value);
    }

    /// <summary>
    /// Builds an instance from enumerable.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbTelemetryContextName FromEnumerable(IEnumerable<byte> value)
    {
        return new EvDbTelemetryContextName(value);
    }

    /// <summary>
    /// Builds an instance from span.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbTelemetryContextName FromSpan(ReadOnlySpan<byte> value)
    {
        return new EvDbTelemetryContextName(value);
    }

    /// <summary>
    /// Builds an instance from json.
    /// </summary>
    /// <param name = "value">The underlying type.</param>
    /// <param name="options"></param>
    /// <returns>An instance of this type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbTelemetryContextName FromJson(JsonDocument value, JsonSerializerOptions? options = null)
    {
        return FromJson(value.RootElement, options);
    }

    /// <summary>
    /// Builds an instance from json.
    /// </summary>
    /// <param name = "value">The underlying type.</param>
    /// <param name="options"></param>
    /// <returns>An instance of this type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbTelemetryContextName FromJson(JsonElement value, JsonSerializerOptions? options = null)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, options);
        return new EvDbTelemetryContextName(bytes);
    }

    #endregion //  From

    #region this[int]

    /// <summary>
    /// Gets the byte at the specified index in the underlying Byte[].
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public byte this[int index] => _value[index];

    #endregion //  this[int]

    #region this[Range]

    /// <summary>
    /// Gets a slice of the underlying Byte[] based on the specified range.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public ReadOnlySpan<byte> this[Range range]
    {
        get
        {
            var (offset, length) = range.GetOffsetAndLength(_value.Length);
            // Use ImmutableArray's efficient slicing
            ReadOnlySpan<byte> slice = _value.AsSpan().Slice(offset, length);
            return slice;
        }
    }

    #endregion //  this[Range]

    #region AsSpan

    /// <summary>
    /// <![CDATA[Gets the underlying Byte[] as a ReadOnlySpan<byte>.]]>
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<byte> AsSpan()
    {
        return (_value?? []).AsSpan();
    }

    #endregion //  AsSpan

    #region IsInitialized

#if !VOGEN_NO_VALIDATION
    private readonly bool _isInitialized;
#endif

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member because of nullability attributes.


    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#if VOGEN_NO_VALIDATION
#pragma warning disable CS8775
public readonly bool IsInitialized() => true;
#pragma warning restore CS8775
#else
    public readonly bool IsInitialized() => _isInitialized;
#endif

    #endregion //  IsInitialized

    #region Casting Overload

    public static implicit operator ReadOnlySpan<byte>(EvDbTelemetryContextName vo) => vo._value.AsSpan();
    public static implicit operator byte[](EvDbTelemetryContextName vo) => vo._value;

    public static implicit operator EvDbTelemetryContextName(byte[] value) => new EvDbTelemetryContextName(value);
    public static implicit operator EvDbTelemetryContextName(Span<byte> value)
    {
        return new EvDbTelemetryContextName(value);
    }

    #endregion //  Casting Overload

    #region Equals, ==

    /// <summary>
    /// Slower comparison that compare the JSON representation of the two instances.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public readonly bool JsonEquals(EvDbTelemetryContextName other)
    {
        // It's possible to create uninitialized instances via converters such as EfCore (HasDefaultValue), which call Equals.
        // We treat anything uninitialized as not equal to anything, even other uninitialized instances of this type.
        if (!IsInitialized() || !other.IsInitialized())
            return false;
        if (IsEquals(_value, other._value))
            return true;
        var selfJson = ToJson();
        var otherJson = other.ToJson();
        var selfJsonText = JsonSerializer.Serialize(selfJson, JSON_COMPARE_OPTIONS);
        var otherJsonText = JsonSerializer.Serialize(otherJson, JSON_COMPARE_OPTIONS);
        return selfJsonText.Equals(otherJsonText, StringComparison.OrdinalIgnoreCase);
    }

    public readonly bool Equals(EvDbTelemetryContextName other)
    {
        // It's possible to create uninitialized instances via converters such as EfCore (HasDefaultValue), which call Equals.
        // We treat anything uninitialized as not equal to anything, even other uninitialized instances of this type.
        if (!IsInitialized() || !other.IsInitialized())
            return false;
        return IsEquals(_value, other._value);
    }

    public readonly bool Equals(byte[] other)
    {
        // It's possible to create uninitialized instances via converters such as EfCore (HasDefaultValue), which call Equals.
        // We treat anything uninitialized as not equal to anything, even other uninitialized instances of this type.
        if (!IsInitialized())
            return false;
        return IsEquals(_value, other);
    }


    public readonly bool Equals(ReadOnlySpan<byte> other)
    {
        if (!IsInitialized())
            return false;
        return IsEquals(_value.AsSpan(), other);
    }

    public readonly override bool Equals(System.Object? obj)
    {
        return obj switch
        {
            EvDbTelemetryContextName item => Equals(item),
            byte[] items => Equals(items),
            ICollection<byte> items => IsEqualsCollection(this, items),
            _ => false
        };
    }

    private static bool IsEquals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        if (a.Length != b.Length)
            return false;
        return a.SequenceEqual(b);
    }

    private static bool IsEqualsCollection(EvDbTelemetryContextName a, ICollection<byte> b)
    {
        if (!a.IsInitialized())
            return false;
        if (a.Length != b.Count)
            return false;
        return a.SequenceEqual(b);
    }

    public static bool operator ==(EvDbTelemetryContextName left, EvDbTelemetryContextName right) => left.Equals(right);
    public static bool operator !=(EvDbTelemetryContextName left, EvDbTelemetryContextName right) => !(left == right);
    public static bool operator ==(EvDbTelemetryContextName left, Byte[] right) => left.Equals(right);
    public static bool operator ==(Byte[] left, EvDbTelemetryContextName right) => right.Equals(left);
    public static bool operator !=(Byte[] left, EvDbTelemetryContextName right) => !(left == right);
    public static bool operator !=(EvDbTelemetryContextName left, Byte[] right) => !(left == right);

    #endregion //  Equals, ==

    #region Parse

    public static EvDbTelemetryContextName Parse(
        [StringSyntax("Json")]
        string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);

        return new EvDbTelemetryContextName(bytes);
    }

    public static EvDbTelemetryContextName Parse(
        [StringSyntax("Json")]
        ReadOnlySequence<char> json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        return new EvDbTelemetryContextName(bytes);
    }

    public static EvDbTelemetryContextName Parse(
        [StringSyntax("Json")]
        ReadOnlyMemory<char> json)
    {
        return Parse(json.Span);
    }

    public static EvDbTelemetryContextName Parse(
        [StringSyntax("Json")]
        ReadOnlySpan<char> json)
    {
        int byteCount = Encoding.UTF8.GetByteCount(json);
        byte[] result = new byte[byteCount];

        Encoding.UTF8.GetBytes(json, result);
        return new EvDbTelemetryContextName(result);
    }

    #endregion //  TryParse

    #region GetHashCode

    public readonly override int GetHashCode() => _value.GetHashCode();

    #endregion //  GetHashCode

    #region ToJson

    public JsonElement ToJson(JsonDocumentOptions options = default)
    {
        ReadOnlyMemory<byte> mem = _value.AsMemory();
        var doc = JsonDocument.Parse(mem, options);
        return doc.RootElement;
    }

    #endregion //  ToJson

    #region ToString

    public override System.String ToString() => ToString(null);

    public System.String ToString(
        [System.Diagnostics.CodeAnalysis.StringSyntaxAttribute("NumericFormat")]
        string? format)
    {
        if (!_isInitialized)
            return "[UNINITIALIZED]";

        JsonElement json = ToJson();
        string result = format switch
        {
            "i" => JsonSerializer.Serialize(json, new JsonSerializerOptions
            {
                WriteIndented = true
            }),
            _ => json.GetRawText()
        };
        return result;
    }

    #endregion //  ToString

    #region IEnumerable

    IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
    {
        foreach (var item in _value ?? [])
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<byte>)this).GetEnumerator();
    }

    #endregion //  IEnumerable

#nullable disable

    #region EvDbTelemetryContextNameSystemTextJsonConverter

    /// <summary>
    /// Converts a EvDbTelemetryContextName to or from JSON.
    /// </summary>
    public class EvDbTelemetryContextNameSystemTextJsonConverter :
        JsonConverter<EvDbTelemetryContextName>
    {
        public override EvDbTelemetryContextName Read(ref Utf8JsonReader reader,
                                                      Type typeToConvert,
                                                      JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<byte[]>(ref reader, options);
            return new EvDbTelemetryContextName(value);
        }

        public override void Write(Utf8JsonWriter writer,
                                   EvDbTelemetryContextName value,
                                   JsonSerializerOptions options)
        {
            IEvDbPayloadRawData self = value;
            JsonSerializer.Serialize(writer, self.RawValue, options);
        }
    }

    #endregion //  EvDbTelemetryContextNameSystemTextJsonConverter

#nullable restore
}

