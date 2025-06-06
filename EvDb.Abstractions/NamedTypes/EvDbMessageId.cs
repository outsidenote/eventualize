using EvDb.Core.NamedTypes.Internals;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EvDb.Core;

/// <summary>
/// Represents a outbox's message tagging into semantic channel name.
/// Part of the message metadata
/// </summary>
[ExcludeFromCodeCoverage]
[JsonConverter(typeof(EvDbMessageIdSystemTextJsonConverter))]
[TypeConverter(typeof(EvDbMessageIdTypeConverter))]
[DebuggerTypeProxy(typeof(EvDbMessageIdDebugView))]
[DebuggerDisplay("{ _value }")]
public partial struct EvDbMessageId :
    IEquatable<EvDbMessageId>,
    IEquatable<Guid>,
    IComparable<EvDbMessageId>,
    IComparable,
    IParsable<EvDbMessageId>
{
    public static readonly EvDbMessageId Empty = new EvDbMessageId(Guid.Empty);

    #region StackTrace? _stackTrace = null!;

#if DEBUG
    private readonly StackTrace? _stackTrace = null!;
#endif

    #endregion //  StackTrace? _stackTrace = null!;

    #region readonly Guid Value { get; }

    private readonly Guid _value = Guid.Empty;
    /// <summary>
    /// Gets the underlying <see cref = "Guid"/> value if set, otherwise a <see cref = "ValueObjectValidationException"/> is thrown.
    /// </summary>
    public readonly Guid Value
    {
        [DebuggerStepThroughAttribute]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            EnsureInitialized();
            return _value!;
        }
    }

    #endregion //  readonly Guid Value { get; }

    #region Ctor

    [DebuggerStepThroughAttribute]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public EvDbMessageId()
    {
#if DEBUG
        _stackTrace = new StackTrace();
#endif
        _isInitialized = false;
        _value = Guid.Empty;
    }

    [DebuggerStepThroughAttribute]
    private EvDbMessageId(Guid value)
    {
        _value = value;
        _isInitialized = true;
    }

    #endregion //  Ctor

    #region From

    /// <summary>
    /// Builds an instance from the provided underlying type.
    /// </summary>
    /// <param name = "value">The underlying type.</param>
    /// <returns>An instance of this type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbMessageId From(Guid value)
    {
        return new EvDbMessageId(value);
    }

    #endregion // From

    #region Initialization

    private readonly bool _isInitialized;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsInitialized() => _isInitialized;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    #endregion //  Initialization

    #region Equals / CompaareTo / GetHashCode

    public readonly bool Equals(EvDbMessageId other)
    {
        // It's possible to create uninitialized instances via converters such as EfCore (HasDefaultValue), which call Equals.
        // We treat anything uninitialized as not equal to anything, even obj uninitialized instances of this type.
        if (!IsInitialized() || !other.IsInitialized())
            return false;
        return EqualityComparer<Guid>.Default.Equals(Value, other.Value);
    }

    public bool Equals(EvDbMessageId other, IEqualityComparer<EvDbMessageId> comparer)
    {
        return comparer.Equals(this, other);
    }

    public readonly bool Equals(Guid primitive)
    {
        return Value.Equals(primitive);
    }

    public readonly bool Equals(Guid primitive, StringComparer comparer)
    {
        return comparer.Equals(Value, primitive);
    }

    public readonly override bool Equals(Object? obj)
    {
        return obj is EvDbMessageId && Equals((EvDbMessageId)obj);
    }

    public int CompareTo(EvDbMessageId other) => Value.CompareTo(other.Value);
    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;
        if (obj is EvDbMessageId x)
            return CompareTo(x);
        ThrowHelper.ThrowArgumentException("Cannot compare to object as it is not of type EvDbMessageId", nameof(obj));
        return 0;
    }

    public readonly override Int32 GetHashCode()
    {
        return EqualityComparer<Guid>.Default.GetHashCode(Value);
    }

    #endregion //  Equals / CompaareTo / GetHashCode

    #region Operator Overloads

    public static implicit operator Guid(EvDbMessageId vo) => vo._value!;
    public static implicit operator EvDbMessageId(Guid value)
    {
        return EvDbMessageId.From(value);
    }

    public static bool operator ==(EvDbMessageId left, EvDbMessageId right) => left.Equals(right);
    public static bool operator !=(EvDbMessageId left, EvDbMessageId right) => !(left == right);
    public static bool operator ==(EvDbMessageId left, Guid? right) => left.Value.Equals(right);
    public static bool operator ==(Guid? left, EvDbMessageId right) => right.Value.Equals(left);
    public static bool operator !=(Guid? left, EvDbMessageId right) => !(left == right);
    public static bool operator !=(EvDbMessageId left, Guid? right) => !(left == right);

    public static bool operator <(EvDbMessageId left, EvDbMessageId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(EvDbMessageId left, EvDbMessageId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(EvDbMessageId left, EvDbMessageId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(EvDbMessageId left, EvDbMessageId right)
    {
        return left.CompareTo(right) >= 0;
    }

    #endregion //  Operator Overloads

    #region Parse / TryParse

    /// <summary>
    /// </summary>
    /// <returns>
    /// True if the value passes any validation (after running any optional normalization).
    /// </returns>
    public static bool TryParse(
        [NotNullWhen(true)]
        string? s,
        [MaybeNullWhen(false)]
        out EvDbMessageId result)
    {
        return TryParse(s, null, out result);
    }

    /// <summary>
    /// </summary>
    /// <returns>
    /// True if the value passes any validation (after running any optional normalization).
    /// </returns>
    public static bool TryParse(
        [NotNullWhen(true)]
        string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)]
        out EvDbMessageId result)
    {
        if (Guid.TryParse(s, provider, out Guid guidValue))
        {
            result = EvDbMessageId.Empty;
            return false;
        }


        result = new EvDbMessageId(guidValue);
        return true;
    }


    /// <summary>
    /// </summary>
    /// <returns>
    /// The value created via the <see cref = "From(Guid)"/> method.
    /// </returns>
    public static EvDbMessageId Parse(string s, IFormatProvider? provider = null)
    {
        Guid guidValue = Guid.Parse(s, provider);
        return From(guidValue);
    }

    #endregion //  Parse / TryParse

    /// <summary>Returns the Guid representation of the underlying <see cref = "Guid"/>.</summary>
    public readonly override string ToString() =>
                    IsInitialized() ? _value.ToString("N") : "[UNINITIALIZED]";

    #region EvDbMessageIdSystemTextJsonConverter

#nullable disable
    /// <summary>
    /// Converts a EvDbMessageId to or from JSON.
    /// </summary>
    public class EvDbMessageIdSystemTextJsonConverter : JsonConverter<EvDbMessageId>
    {
        public override EvDbMessageId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return EvDbMessageId.From(reader.GetGuid());
        }

        public override void Write(Utf8JsonWriter writer, EvDbMessageId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }

#nullable restore

    #endregion //  EvDbMessageIdSystemTextJsonConverter

#nullable disable
    #region EvDbMessageIdTypeConverter

    class EvDbMessageIdTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(Guid) || base.CanConvertFrom(context, sourceType);
        }

        public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
        {
            var result = value switch
            {
                Guid guidValue => From(guidValue),
                string strValue when EvDbMessageId.TryParse(strValue, out var guid) => guid,
                _ => base.ConvertFrom(context, culture, value)
            };

            return result;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(Guid) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
        {
            if (value is EvDbMessageId idValue && destinationType == typeof(Guid))
            {
                return idValue.Value;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion //  EvDbMessageIdTypeConverter

    #region EvDbMessageIdDebugView

#nullable restore
#nullable disable
#pragma warning disable S3453 // Classes should not have only "private" constructors
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable S1144 // Unused private types or members should be removed
    internal sealed class EvDbMessageIdDebugView
    {
        private readonly EvDbMessageId _t;
        EvDbMessageIdDebugView(EvDbMessageId t)
        {
            _t = t;
        }

        public bool IsInitialized => _t.IsInitialized();
        public string UnderlyingType => "Guid";
        public string Value => _t.IsInitialized() ? _t._value.ToString() : "[not initialized]";
#if DEBUG
        public string CreatedWith => _t._stackTrace?.ToString() ?? "the From method";
#endif
        public string Conversions => @"Default";
    }
#pragma warning restore S3453 // Classes should not have only "private" constructors
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore S1144 // Unused private types or members should be removed
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static

    #endregion //  EvDbMessageIdDebugView
#nullable restore
}
