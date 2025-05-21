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
/// Name of stream root address (represent the uniqueness of the stream)
/// along with the stream id it represent the stream's address.
/// adding the offset we can get a stream's cursor
/// </summary>
[ExcludeFromCodeCoverage]
[JsonConverter(typeof(EvDbStreamTypeNameSystemTextJsonConverter))]
[TypeConverter(typeof(EvDbStreamTypeNameTypeConverter))]
[DebuggerTypeProxy(typeof(EvDbStreamTypeNameDebugView))]
[DebuggerDisplay("{ _value }")]
public partial struct EvDbStreamTypeName :
    IEquatable<EvDbStreamTypeName>,
    IEquatable<string>,
    IComparable<EvDbStreamTypeName>,
    IComparable,
    IParsable<EvDbStreamTypeName>
{
    #region Validation

    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9_\-\./:]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex Validator();

    private static Validation Validate(string value) => Validator().IsMatch(value) switch
    {

        true => Validation.Ok,
        _ => Validation.Invalid(@"The address name must start with letters (A-Z) or (a-z), follow by alphabets, numbers, or the characters `-`, `_`, `.`, `:`, `/`")
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
    public EvDbStreamTypeName()
    {
#if DEBUG
        _stackTrace = new StackTrace();
#endif
        _isInitialized = false;
        _value = default;
    }

    [DebuggerStepThroughAttribute]
    private EvDbStreamTypeName(string value)
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
    public static EvDbStreamTypeName From(string value)
    {
        value = Format(value);
        var validation = EvDbStreamTypeName.Validate(value);
        if (validation != Validation.Ok)
        {
            ThrowHelper.ThrowWhenValidationFails(validation);
        }

        return new EvDbStreamTypeName(value);
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
                            out EvDbStreamTypeName vo)
    {
        if (value is null)
        {
            vo = default;
            return false;
        }

        var validation = EvDbStreamTypeName.Validate(value);
        if (validation != Validation.Ok)
        {
            vo = default!;
            return false;
        }

        vo = new EvDbStreamTypeName(value);
        return true;
    }

    /// <summary>
    /// Tries to build an instance from the provided underlying value.
    /// If a normalization method is provided, it will be called.
    /// If validation is provided, and it fails, an error will be returned.
    /// </summary>
    /// <param name = "value">The primitive value.</param>
    /// <returns>A <see cref = "ValueObjectOrError{T}"/> containing either the value object, or an error.</returns>
    public static ValueObjectOrError<EvDbStreamTypeName> TryFrom(string value)
    {
        if (value is null)
        {
            return new ValueObjectOrError<EvDbStreamTypeName>(Validation.Invalid("The value provided was null"));
        }

        value = Format(value);
        var validation = EvDbStreamTypeName.Validate(value);
        if (validation != Validation.Ok)
        {
            return new ValueObjectOrError<EvDbStreamTypeName>(validation);
        }

        return new ValueObjectOrError<EvDbStreamTypeName>(new EvDbStreamTypeName(value));
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
#pragma warning disable S4144 // Methods should not have identical implementations
    private static EvDbStreamTypeName __Deserialize(string value)
    {
        value = Format(value);
        var validation = EvDbStreamTypeName.Validate(value);
        if (validation != Validation.Ok)
        {
            ThrowHelper.ThrowWhenValidationFails(validation);
        }

        return new EvDbStreamTypeName(value);
    }
#pragma warning restore S4144 // Methods should not have identical implementations

    #endregion //  __Deserialize

    #region Equals / CompaareTo / GetHashCode

    public readonly bool Equals(EvDbStreamTypeName other)
    {
        // It's possible to create uninitialized instances via converters such as EfCore (HasDefaultValue), which call Equals.
        // We treat anything uninitialized as not equal to anything, even obj uninitialized instances of this type.
        if (!IsInitialized() || !other.IsInitialized())
            return false;
        return EqualityComparer<string>.Default.Equals(Value, other.Value);
    }

    public bool Equals(EvDbStreamTypeName other, IEqualityComparer<EvDbStreamTypeName> comparer)
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
        return obj is EvDbStreamTypeName && Equals((EvDbStreamTypeName)obj);
    }

    public int CompareTo(EvDbStreamTypeName other) => Value.CompareTo(other.Value);
    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;
        if (obj is EvDbStreamTypeName x)
            return CompareTo(x);
        ThrowHelper.ThrowArgumentException("Cannot compare to object as it is not of type EvDbStreamTypeName", nameof(obj));
        return 0;
    }

    public readonly override Int32 GetHashCode()
    {
        return EqualityComparer<string>.Default.GetHashCode(Value);
    }

    #endregion //  Equals / CompaareTo / GetHashCode

    #region Operator Overloads

    public static implicit operator string(EvDbStreamTypeName vo) => vo._value!;
    public static implicit operator EvDbStreamTypeName(string value)
    {
        return EvDbStreamTypeName.From(value);
    }

    public static bool operator ==(EvDbStreamTypeName left, EvDbStreamTypeName right) => left.Equals(right);
    public static bool operator !=(EvDbStreamTypeName left, EvDbStreamTypeName right) => !(left == right);
    public static bool operator ==(EvDbStreamTypeName left, string? right) => left.Value.Equals(right);
    public static bool operator ==(string? left, EvDbStreamTypeName right) => right.Value.Equals(left);
    public static bool operator !=(string? left, EvDbStreamTypeName right) => !(left == right);
    public static bool operator !=(EvDbStreamTypeName left, string? right) => !(left == right);

    public static bool operator <(EvDbStreamTypeName left, EvDbStreamTypeName right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(EvDbStreamTypeName left, EvDbStreamTypeName right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(EvDbStreamTypeName left, EvDbStreamTypeName right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(EvDbStreamTypeName left, EvDbStreamTypeName right)
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
        out EvDbStreamTypeName result)
    {
        if (s is null)
        {
            result = default;
            return false;
        }

        var validation = EvDbStreamTypeName.Validate(s);
        if (validation != Validation.Ok)
        {
            result = default;
            return false;
        }

        result = new EvDbStreamTypeName(s);
        return true;
    }

    /// <summary>
    /// </summary>
    /// <returns>
    /// The value created via the <see cref = "From(string)"/> method.
    /// </returns>
    public static EvDbStreamTypeName Parse(string s, IFormatProvider? provider)
    {
        return From(s!);
    }

    #endregion //  Parse / TryParse

    /// <summary>Returns the string representation of the underlying <see cref = "string"/>.</summary>
    public readonly override string ToString() =>
                    IsInitialized() ? Value.ToString() : "[UNINITIALIZED]";

    #region EvDbStreamTypeNameSystemTextJsonConverter

#nullable disable
    /// <summary>
    /// Converts a EvDbStreamTypeName to or from JSON.
    /// </summary>
    public class EvDbStreamTypeNameSystemTextJsonConverter : JsonConverter<EvDbStreamTypeName>
    {
        public override EvDbStreamTypeName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return EvDbStreamTypeName.__Deserialize(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, EvDbStreamTypeName value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }

        public override EvDbStreamTypeName ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return EvDbStreamTypeName.__Deserialize(reader.GetString());
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, EvDbStreamTypeName value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.Value);
        }
    }

#nullable restore

    #endregion //  EvDbStreamTypeNameSystemTextJsonConverter

#nullable disable
    #region EvDbStreamTypeNameTypeConverter

    class EvDbStreamTypeNameTypeConverter : TypeConverter
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
                return EvDbStreamTypeName.__Deserialize(stringValue);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
        {
            if (value is EvDbStreamTypeName idValue && destinationType == typeof(string))
            {
                return idValue.Value;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion //  EvDbStreamTypeNameTypeConverter
#nullable restore

    #region Validation

    public class Validation
    {
        public string ErrorMessage { get; }

        /// <summary>
        /// Contains data related to validation.
        /// </summary>
        public Dictionary<object, object>? Data { get; private set; } = null;

        public static readonly Validation Ok = new Validation(string.Empty);

        private Validation(string reason) => ErrorMessage = reason;

        public static Validation Invalid(string reason = "")
        {
            if (string.IsNullOrEmpty(reason))
            {
                return new Validation("[none provided]");
            }

            return new Validation(reason);
        }

        /// <summary>
        /// Adds the specified data to the validation.
        /// This data will be copied to the Data property of the thrown Exception.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Validation.</returns>
        public Validation WithData(object key, object value)
        {
            Data ??= new();
            Data[key] = value;
            return this;
        }
    }

    #endregion //  Validation

#nullable disable

    #region ValueObjectValidationException

    [Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ValueObjectValidationException : Exception
    {
        public ValueObjectValidationException()
        {
        }

        public ValueObjectValidationException(string message) : base(message)
        {
        }

        public ValueObjectValidationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly

    #endregion //  ValueObjectValidationException

    #region EvDbStreamTypeNameDebugView

#nullable restore
#nullable disable
#pragma warning disable S3453 // Classes should not have only "private" constructors
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable S1144 // Unused private types or members should be removed
    internal sealed class EvDbStreamTypeNameDebugView
    {
        private readonly EvDbStreamTypeName _t;
        EvDbStreamTypeNameDebugView(EvDbStreamTypeName t)
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

    #endregion //  EvDbStreamTypeNameDebugView

    #region ThrowHelper

#nullable restore
    static class ThrowHelper
    {
        [DoesNotReturnAttribute]
        internal static void ThrowInvalidOperationException(string message) => throw new InvalidOperationException(message);
        [DoesNotReturnAttribute]
        internal static void ThrowArgumentException(string message, string arg) => throw new ArgumentException(message, arg);
        [DoesNotReturnAttribute]
        internal static void ThrowWhenCreatedWithNull() => throw new ArgumentException("Cannot create a value object with null.");
        [DoesNotReturnAttribute]
        internal static void ThrowWhenNotInitialized() => throw new ArgumentException("Use of uninitialized Value Object.");
        [DoesNotReturnAttribute]
        internal static void ThrowWhenNotInitialized(StackTrace? stackTrace) => throw new ArgumentException("Use of uninitialized Value Object at: " + stackTrace);
        [DoesNotReturnAttribute]
        internal static void ThrowWhenValidationFails(Validation validation)
        {
            var ex = new ValueObjectValidationException(validation.ErrorMessage);
            if (validation.Data is not null)
            {
                foreach (var kvp in validation.Data)
                {
                    ex.Data[kvp.Key] = kvp.Value;
                }
            }

            throw ex;
        }
    }

    #endregion //  ThrowHelper

    #region ValueObjectOrError

    public sealed class ValueObjectOrError<T>
    {
        private readonly bool _isSuccess;

        private readonly T? _valueObject;

        private readonly Validation? _error;

        public bool IsSuccess => _isSuccess;

        public Validation Error
        {
            get
            {
                if (!_isSuccess)
                {
                    return _error!;
                }

                return Validation.Ok;
            }
        }

        public T ValueObject
        {
            get
            {
                if (!_isSuccess)
                {
                    throw new InvalidOperationException("Cannot access the value object as it is not valid: " + _error?.ErrorMessage);
                }

                return _valueObject!;
            }
        }

        public ValueObjectOrError(T valueObject)
        {
            _isSuccess = true;
            _valueObject = valueObject;
            _error = Validation.Ok;
        }

        public ValueObjectOrError(Validation error)
        {
            _isSuccess = false;
            _valueObject = default(T);
            _error = error;
        }
    }

    #endregion //  ValueObjectOrError
}
#nullable restore
