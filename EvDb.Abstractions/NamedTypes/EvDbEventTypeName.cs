using EvDb.Core.NamedTypes.Internals;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace EvDb.Core;

/// <summary>
/// Represents a outbox's message tagging into semantic channel name.
/// Part of the message metadata
/// </summary>
[ExcludeFromCodeCoverage]
[JsonConverter(typeof(EvDbEventTypeNameSystemTextJsonConverter))]
[TypeConverter(typeof(EvDbEventTypeNameTypeConverter))]
[DebuggerTypeProxy(typeof(EvDbEventTypeNameDebugView))]
[DebuggerDisplay("{ _value }")]
public partial struct EvDbEventTypeName :
    IEquatable<EvDbEventTypeName>,
    IEquatable<string>,
    IComparable<EvDbEventTypeName>,
    IComparable,
    IParsable<EvDbEventTypeName>
{
    #region Validation

    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9_\.\-@#]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex Validator();

    private static Validation Validate(string value) => Validator().IsMatch(value) switch
    {

        true => Validation.Ok,
        _ => Validation.Invalid("The channel name must start with letters (A-Z) or (a-z), follow by alphabets, numbers, or the characters `-`, `.`, `_`, `@`, or `#`     .")
    };

    #endregion //  Validation

    #region StackTrace? _stackTrace = null!;

#if DEBUG
    private readonly StackTrace? _stackTrace = null!;
#endif

    #endregion //  StackTrace? _stackTrace = null!;

    #region readonly string Value { get; }

    private readonly string? _value;
    /// <summary>
    /// Gets the underlying <see cref = "string"/> value if set, otherwise a <see cref = "ValueObjectValidationException"/> is thrown.
    /// </summary>
    public readonly string Value
    {
        [DebuggerStepThroughAttribute]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            EnsureInitialized();
            return _value!;
        }
    }

    #endregion //  readonly string Value { get; }

    #region Ctor

    [DebuggerStepThroughAttribute]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public EvDbEventTypeName()
    {
#if DEBUG
        _stackTrace = new StackTrace();
#endif
        _isInitialized = false;
        _value = default;
    }

    [DebuggerStepThroughAttribute]
    private EvDbEventTypeName(string value)
    {
        _value = value;
        _isInitialized = true;
    }

    #endregion //  Ctor

    private static string Format(string value) => value;


    #region TryFrom / From

    /// <summary>
    /// Builds an instance from the provided underlying type.
    /// </summary>
    /// <param name = "value">The underlying type.</param>
    /// <returns>An instance of this type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvDbEventTypeName From(string value)
    {
        value = Format(value);
        var validation = EvDbEventTypeName.Validate(value);
        if (validation != Validation.Ok)
        {
            ThrowHelper.ThrowWhenValidationFails(validation);
        }

        return new EvDbEventTypeName(value);
    }

    /// <summary>
    /// Tries to build an instance from the provided underlying type.
    /// If a normalization method is provided, it will be called.
    /// If validation is provided, and it fails, false will be returned.
    /// </summary>
    /// <param name = "value">The underlying type.</param>
    /// <param name = "vo">An instance of the value object.</param>
    /// <returns>True if the value object can be built, otherwise false.</returns>
    public static bool TryFrom(
                            [NotNullWhen(true)]
                            string? value,
                            [MaybeNullWhen(false)]
                            out EvDbEventTypeName vo)
    {
        if (value is null)
        {
            vo = default;
            return false;
        }

        var validation = EvDbEventTypeName.Validate(value);
        if (validation != Validation.Ok)
        {
            vo = default!;
            return false;
        }

        vo = new EvDbEventTypeName(value);
        return true;
    }

    /// <summary>
    /// Tries to build an instance from the provided underlying value.
    /// If a normalization method is provided, it will be called.
    /// If validation is provided, and it fails, an error will be returned.
    /// </summary>
    /// <param name = "value">The primitive value.</param>
    /// <returns>A <see cref = "ValueObjectOrError{T}"/> containing either the value object, or an error.</returns>
    public static ValueObjectOrError<EvDbEventTypeName> TryFrom(string value)
    {
        if (value is null)
        {
            return new ValueObjectOrError<EvDbEventTypeName>(Validation.Invalid("The value provided was null"));
        }

        value = Format(value);
        var validation = EvDbEventTypeName.Validate(value);
        if (validation != Validation.Ok)
        {
            return new ValueObjectOrError<EvDbEventTypeName>(validation);
        }

        return new ValueObjectOrError<EvDbEventTypeName>(new EvDbEventTypeName(value));
    }

    #endregion //  TryFrom / From

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

    #region __Deserialize

    // only called internally when something has been deserialized into
    // its primitive type.
    private static EvDbEventTypeName __Deserialize(string value)
    {
        value = Format(value);
        var validation = EvDbEventTypeName.Validate(value);
        if (validation != Validation.Ok)
        {
            ThrowHelper.ThrowWhenValidationFails(validation);
        }

        return new EvDbEventTypeName(value);
    }

    #endregion //  __Deserialize

    #region Equals / CompaareTo / GetHashCode

    public readonly bool Equals(EvDbEventTypeName other)
    {
        // It's possible to create uninitialized instances via converters such as EfCore (HasDefaultValue), which call Equals.
        // We treat anything uninitialized as not equal to anything, even obj uninitialized instances of this type.
        if (!IsInitialized() || !other.IsInitialized())
            return false;
        return EqualityComparer<string>.Default.Equals(Value, other.Value);
    }

    public bool Equals(EvDbEventTypeName other, IEqualityComparer<EvDbEventTypeName> comparer)
    {
        return comparer.Equals(this, other);
    }

    public readonly bool Equals(string? primitive)
    {
        return Value.Equals(primitive);
    }

    public readonly bool Equals(string? primitive, StringComparer comparer)
    {
        return comparer.Equals(Value, primitive);
    }

    public readonly override bool Equals(Object? obj)
    {
        return obj is EvDbEventTypeName && Equals((EvDbEventTypeName)obj);
    }

    public int CompareTo(EvDbEventTypeName other) => Value.CompareTo(other.Value);
    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;
        if (obj is EvDbEventTypeName x)
            return CompareTo(x);
        ThrowHelper.ThrowArgumentException("Cannot compare to object as it is not of type EvDbEventTypeName", nameof(obj));
        return 0;
    }

    public readonly override Int32 GetHashCode()
    {
        return EqualityComparer<string>.Default.GetHashCode(Value);
    }

    #endregion //  Equals / CompaareTo / GetHashCode

    #region Operator Overloads

    public static implicit operator string(EvDbEventTypeName vo) => vo._value!;
    public static implicit operator EvDbEventTypeName(string value)
    {
        return EvDbEventTypeName.From(value);
    }

    public static bool operator ==(EvDbEventTypeName left, EvDbEventTypeName right) => left.Equals(right);
    public static bool operator !=(EvDbEventTypeName left, EvDbEventTypeName right) => !(left == right);
    public static bool operator ==(EvDbEventTypeName left, string? right) => left.Value.Equals(right);
    public static bool operator ==(string? left, EvDbEventTypeName right) => right.Value.Equals(left);
    public static bool operator !=(string? left, EvDbEventTypeName right) => !(left == right);
    public static bool operator !=(EvDbEventTypeName left, string? right) => !(left == right);

    public static bool operator <(EvDbEventTypeName left, EvDbEventTypeName right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(EvDbEventTypeName left, EvDbEventTypeName right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(EvDbEventTypeName left, EvDbEventTypeName right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(EvDbEventTypeName left, EvDbEventTypeName right)
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
        IFormatProvider? provider,
        [MaybeNullWhen(false)]
        out EvDbEventTypeName result)
    {
        if (s is null)
        {
            result = default;
            return false;
        }

        var validation = EvDbEventTypeName.Validate(s);
        if (validation != Validation.Ok)
        {
            result = default;
            return false;
        }

        result = new EvDbEventTypeName(s);
        return true;
    }

    /// <summary>
    /// </summary>
    /// <returns>
    /// The value created via the <see cref = "From(string)"/> method.
    /// </returns>
    public static EvDbEventTypeName Parse(string s, IFormatProvider? provider)
    {
        return From(s!);
    }

    #endregion //  Parse / TryParse

    /// <summary>Returns the string representation of the underlying <see cref = "string"/>.</summary>
    public readonly override string ToString() =>
                    IsInitialized() ? Value.ToString() : "[UNINITIALIZED]";

    #region EvDbEventTypeNameSystemTextJsonConverter

#nullable disable
    /// <summary>
    /// Converts a EvDbEventTypeName to or from JSON.
    /// </summary>
    public class EvDbEventTypeNameSystemTextJsonConverter : JsonConverter<EvDbEventTypeName>
    {
        public override EvDbEventTypeName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return EvDbEventTypeName.__Deserialize(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, EvDbEventTypeName value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }

        public override EvDbEventTypeName ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return EvDbEventTypeName.__Deserialize(reader.GetString());
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, EvDbEventTypeName value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.Value);
        }
    }

#nullable restore

    #endregion //  EvDbEventTypeNameSystemTextJsonConverter

#nullable disable
    #region EvDbEventTypeNameTypeConverter

    class EvDbEventTypeNameTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
        {
            var stringValue = value as string;
            if (stringValue is not null)
            {
                return EvDbEventTypeName.__Deserialize(stringValue);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
        {
            if (value is EvDbEventTypeName idValue && destinationType == typeof(string))
            {
                return idValue.Value;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion //  EvDbEventTypeNameTypeConverter

    #region EvDbEventTypeNameDebugView

#nullable restore
#nullable disable
#pragma warning disable S3453 // Classes should not have only "private" constructors
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable S1144 // Unused private types or members should be removed
    internal sealed class EvDbEventTypeNameDebugView
    {
        private readonly EvDbEventTypeName _t;
        EvDbEventTypeNameDebugView(EvDbEventTypeName t)
        {
            _t = t;
        }

        public bool IsInitialized => _t.IsInitialized();
        public string UnderlyingType => "string";
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

    #endregion //  EvDbEventTypeNameDebugView
#nullable restore
}
