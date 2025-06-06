﻿using EvDb.Core.NamedTypes.Internals;
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
[JsonConverter(typeof(EvDbMessagePayloadNameSystemTextJsonConverter))]
[DebuggerDisplay("{ _value }")]
public readonly partial struct EvDbMessagePayloadName :
    IEquatable<EvDbMessagePayloadName>,
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
            EnsureInitialized();
            return _value;
        }
    }

    #endregion //  IEvDbPayloadRawData.RawValue

    #region Length

    /// <summary>
    /// Gets the length of the underlying Byte[].
    /// </summary>
    public int Length => _value.Length;

    #endregion //  Length

    #region Ctor

    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public EvDbMessagePayloadName()
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
    private EvDbMessagePayloadName(IEnumerable<byte> value)
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
    private EvDbMessagePayloadName(byte[] value)
    {
        _value = value;
#if !VOGEN_NO_VALIDATION
        _isInitialized = true;
#endif
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbMessagePayloadName(ReadOnlySpan<byte> value) : this(value.ToImmutableArray())
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbMessagePayloadName(Span<byte> value) : this(value.ToImmutableArray())
    {
    }

    #endregion //  Ctor

    #region Empty

    /// <summary>
    /// Represents an empty instance of this type.
    /// </summary>
    public static EvDbMessagePayloadName Empty { get; } = new EvDbMessagePayloadName(ImmutableArray<byte>.Empty);

    #endregion //  Empty

    #region From

    /// <summary>
    /// Builds an instance from byte[]. 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbMessagePayloadName FromArray(byte[] value)
    {
        return new EvDbMessagePayloadName(value);
    }

    /// <summary>
    /// Builds an instance from enumerable.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbMessagePayloadName FromEnumerable(IEnumerable<byte> value)
    {
        return new EvDbMessagePayloadName(value);
    }

    /// <summary>
    /// Builds an instance from span.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbMessagePayloadName FromSpan(ReadOnlySpan<byte> value)
    {
        return new EvDbMessagePayloadName(value);
    }

    /// <summary>
    /// Builds an instance from json.
    /// </summary>
    /// <param name = "value">The underlying type.</param>
    /// <param name="options"></param>
    /// <returns>An instance of this type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbMessagePayloadName FromJson(JsonDocument value, JsonSerializerOptions? options = null)
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
    public static EvDbMessagePayloadName FromJson(JsonElement value, JsonSerializerOptions? options = null)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, options);
        return new EvDbMessagePayloadName(bytes);
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
    /// Gets the underlying Byte[] as a ReadOnlySpan<byte>.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<byte> AsSpan()
    {
        EnsureInitialized();
        return _value.AsSpan();
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

    public static implicit operator ReadOnlySpan<byte>(EvDbMessagePayloadName vo) => vo._value.AsSpan();
    public static implicit operator byte[](EvDbMessagePayloadName vo) => vo._value;

    public static implicit operator EvDbMessagePayloadName(byte[] value) => new EvDbMessagePayloadName(value);
    public static implicit operator EvDbMessagePayloadName(Span<byte> value)
    {
        return new EvDbMessagePayloadName(value);
    }

    #endregion //  Casting Overload

    #region Equals, ==

    /// <summary>
    /// Slower comparison that compare the JSON representation of the two instances.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public readonly bool JsonEquals(EvDbMessagePayloadName other)
    {
        // It's possible to create uninitialized instances via converters such as EfCore (HasDefaultValue), which call Equals.
        // We treat anything uninitialized as not equal to anything, even other uninitialized instances of this type.
        if (!IsInitialized() || !other.IsInitialized())
            return false;
        if (IsEquals(_value, other._value))
            return true;
        if (!TryToJson(out var selfJson))
            return false;
        if (!other.TryToJson(out var otherJson))
            return false;
        var selfJsonText = JsonSerializer.Serialize(selfJson, JSON_COMPARE_OPTIONS);
        var otherJsonText = JsonSerializer.Serialize(otherJson, JSON_COMPARE_OPTIONS);
        return selfJsonText.Equals(otherJsonText, StringComparison.OrdinalIgnoreCase);
    }

    public readonly bool Equals(EvDbMessagePayloadName other)
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
            EvDbMessagePayloadName item => Equals(item),
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

    private static bool IsEqualsCollection(EvDbMessagePayloadName a, ICollection<byte> b)
    {
        if (!a.IsInitialized())
            return false;
        if (a.Length != b.Count)
            return false;
        return a.SequenceEqual(b);
    }

    public static bool operator ==(EvDbMessagePayloadName left, EvDbMessagePayloadName right) => left.Equals(right);
    public static bool operator !=(EvDbMessagePayloadName left, EvDbMessagePayloadName right) => !(left == right);
    public static bool operator ==(EvDbMessagePayloadName left, Byte[] right) => left.Equals(right);
    public static bool operator ==(Byte[] left, EvDbMessagePayloadName right) => right.Equals(left);
    public static bool operator !=(Byte[] left, EvDbMessagePayloadName right) => !(left == right);
    public static bool operator !=(EvDbMessagePayloadName left, Byte[] right) => !(left == right);

    #endregion //  Equals, ==

    #region Parse

    public static EvDbMessagePayloadName Parse(
        [StringSyntax("Json")]
        string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);

        return new EvDbMessagePayloadName(bytes);
    }

    public static EvDbMessagePayloadName Parse(
        [StringSyntax("Json")]
        ReadOnlySequence<char> json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        return new EvDbMessagePayloadName(bytes);
    }

    public static EvDbMessagePayloadName Parse(
        [StringSyntax("Json")]
        ReadOnlyMemory<char> json)
    {
        return Parse(json.Span);
    }

    public static EvDbMessagePayloadName Parse(
        [StringSyntax("Json")]
        ReadOnlySpan<char> json)
    {
        int byteCount = Encoding.UTF8.GetByteCount(json);
        byte[] result = new byte[byteCount];

        Encoding.UTF8.GetBytes(json, result);
        return new EvDbMessagePayloadName(result);
    }

    #endregion //  TryParse

    #region GetHashCode

    public readonly override int GetHashCode() => _value.GetHashCode();

    #endregion //  GetHashCode

    #region TryToJson

    /// <summary>
    /// Tries to convert the value to a JsonDocument.
    /// Because of custom serialization, this will not work for all instances.
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public bool TryToJson(
      [NotNullWhen(true)] out JsonDocument? document)
    {
        var reader = new Utf8JsonReader(_value.AsSpan());
        return JsonDocument.TryParseValue(ref reader, out document);
    }

    #endregion //  TryToJson

    #region ToString

    public override System.String ToString() => ToString(null);

    public System.String ToString(
        [System.Diagnostics.CodeAnalysis.StringSyntaxAttribute("NumericFormat")]
        string? format)
    {
        if (!_isInitialized)
            return "[UNINITIALIZED]";

        if (!TryToJson(out var doc))
        {
            var bytes20 = _value.Take(20);
            var str20 = string.Join(",", bytes20);
            if (_value.Length > 20)
            {
                str20 = $"{str20}…";
            }
            return str20;
        }

        JsonElement json = doc.RootElement;
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
        EnsureInitialized();
        foreach (var item in _value)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable<byte>)this).GetEnumerator();

    #endregion //  IEnumerable

    #region EnsureInitialized

    private readonly void EnsureInitialized()
    {
        if (!IsInitialized())
        {
#if DEBUG
            ThrowHelper.ThrowWhenNotInitialized(_stackTrace);
#else
            ThrowHelper.ThrowWhenNotInitialized();
#endif
        }
    }

    #endregion //  EnsureInitialized

#nullable disable

    #region EvDbMessagePayloadNameSystemTextJsonConverter

    /// <summary>
    /// Converts a EvDbMessagePayloadName to or from JSON.
    /// </summary>
    public class EvDbMessagePayloadNameSystemTextJsonConverter :
        JsonConverter<EvDbMessagePayloadName>
    {
        public override EvDbMessagePayloadName Read(ref Utf8JsonReader reader,
                                                      Type typeToConvert,
                                                      JsonSerializerOptions options)
        {
            ImmutableArray<byte> value = JsonSerializer.Deserialize<ImmutableArray<byte>>(ref reader, options);
            return new EvDbMessagePayloadName(value);
        }

        public override void Write(Utf8JsonWriter writer,
                                   EvDbMessagePayloadName value,
                                   JsonSerializerOptions options)
        {
            IEvDbPayloadRawData self = value;
            JsonSerializer.Serialize(writer, self.RawValue, options);
        }
    }

    #endregion //  EvDbMessagePayloadNameSystemTextJsonConverter

#nullable restore
}

