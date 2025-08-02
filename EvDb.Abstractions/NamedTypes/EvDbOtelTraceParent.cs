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
public readonly partial struct EvDbOtelTraceParent :
    IEquatable<EvDbOtelTraceParent>
    //IEvDbPayloadRawData
{
    private static readonly JsonSerializerOptions JSON_COMPARE_OPTIONS = new JsonSerializerOptions { WriteIndented = true };
    #region _stackTrace

#if DEBUG
    private readonly System.Diagnostics.StackTrace? _stackTrace = null!;
#endif

    #endregion //  _stackTrace

    private readonly string? _value;

    #region Ctor

    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public EvDbOtelTraceParent()
    {
#if DEBUG
        _stackTrace = new System.Diagnostics.StackTrace();
#endif
        _isInitialized = false;
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbOtelTraceParent(string? value)
    {
        _value = value;
        _isInitialized = true;
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbOtelTraceParent(IEnumerable<byte> value): this(value.ToArray())
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbOtelTraceParent(byte[] value)
    {
        if(value is not null && value.Any())
            _value = Encoding.UTF8.GetString(value);

        _isInitialized = true;
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbOtelTraceParent(ReadOnlySpan<byte> value) 
    {
        if(value.Length != 0)
            _value = Encoding.UTF8.GetString(value);

        _isInitialized = true;
    }

    [System.Diagnostics.DebuggerStepThroughAttribute]
    private EvDbOtelTraceParent(Span<byte> value)
    {
        if(value.Length != 0)
            _value = Encoding.UTF8.GetString(value);

        _isInitialized = true;
    }

    #endregion //  Ctor

    #region Empty

    /// <summary>
    /// Represents an empty instance of this type.
    /// </summary>
    public static EvDbOtelTraceParent Empty { get; } = new EvDbOtelTraceParent((string?)null);

    #endregion //  Empty

    #region IsEmpty

    /// <summary>
    /// Gets a value indicating whether the current instance is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_value);

    #endregion //  IsEmpty

    #region From

    /// <summary>
    /// Builds an instance from string. 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbOtelTraceParent From(string? value)
    {
        if (value == null)
            return Empty;
        return new EvDbOtelTraceParent(value);
    }

    /// <summary>
    /// Builds an instance from byte[]. 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbOtelTraceParent FromArray(byte[] value)
    {
        if (value == null || value.Length == 0)
            return Empty;
        return new EvDbOtelTraceParent(value);
    }

    /// <summary>
    /// Builds an instance from enumerable.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbOtelTraceParent FromEnumerable(IEnumerable<byte> value)
    {
        return FromArray(value.ToArray());
    }

    /// <summary>
    /// Builds an instance from span.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbOtelTraceParent FromSpan(ReadOnlySpan<byte> value)
    {
        if (value == null || value.Length == 0)
            return Empty;
        return new EvDbOtelTraceParent(value);
    }

    #endregion //  From

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

    public static implicit operator string?(EvDbOtelTraceParent vo) => vo._value;

    public static implicit operator EvDbOtelTraceParent(string? value) => new EvDbOtelTraceParent(value);

    #endregion //  Casting Overload

    #region Equals, ==

    public readonly bool Equals(EvDbOtelTraceParent other)
    {
        // It's possible to create uninitialized instances via converters such as EfCore (HasDefaultValue), which call Equals.
        // We treat anything uninitialized as not equal to anything, even other uninitialized instances of this type.
        if (!IsInitialized() || !other.IsInitialized())
            return false;
        return IsEquals(_value, other._value);
    }

    public readonly override bool Equals(System.Object? obj)
    {
        return obj switch
        {
            EvDbOtelTraceParent item => Equals(item),
            string v => Equals(v),
            _ => false
        };
    }

    private static bool IsEquals(string? a, string? b)
    {
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0 ||
               (a is null && b is null);    
    }

    public static bool operator ==(EvDbOtelTraceParent left, EvDbOtelTraceParent right) => left.Equals(right);
    public static bool operator !=(EvDbOtelTraceParent left, EvDbOtelTraceParent right) => !(left == right);
    public static bool operator ==(EvDbOtelTraceParent left, string? right) => left.Equals(right);
    public static bool operator ==(string? left, EvDbOtelTraceParent right) => right.Equals(left);
    public static bool operator !=(string? left, EvDbOtelTraceParent right) => !(left == right);
    public static bool operator !=(EvDbOtelTraceParent left, string? right) => !(left == right);

    #endregion //  Equals, ==

    #region GetHashCode

    public readonly override int GetHashCode() => _value?.GetHashCode() ?? 0;

    #endregion //  GetHashCode

    #region ToString

    public override System.String ToString() => _value ?? string.Empty;

    #endregion //  ToString


#nullable disable

    #region EvDbTelemetryContextNameSystemTextJsonConverter

    /// <summary>
    /// Converts a EvDbOtelTraceParent to or from JSON.
    /// </summary>
    public class EvDbTelemetryContextNameSystemTextJsonConverter :
        JsonConverter<EvDbOtelTraceParent>
    {
        public override EvDbOtelTraceParent Read(ref Utf8JsonReader reader,
                                                      Type typeToConvert,
                                                      JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return new EvDbOtelTraceParent(value);
        }

        public override void Write(Utf8JsonWriter writer,
                                   EvDbOtelTraceParent value,
                                   JsonSerializerOptions options)
        {
            string? val = value._value;
            if (string.IsNullOrEmpty(val))
            {
                writer.WriteNullValue();
                return;
            }
            writer.WriteStringValue(val);
        }
    }

    #endregion //  EvDbTelemetryContextNameSystemTextJsonConverter

#nullable restore
}

