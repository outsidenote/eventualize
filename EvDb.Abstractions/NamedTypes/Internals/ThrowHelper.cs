using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace EvDb.Core.NamedTypes.Internals;

public static class ThrowHelper
{
    [DoesNotReturnAttribute]
    public static void ThrowInvalidOperationException(string message) => throw new InvalidOperationException(message);
    [DoesNotReturnAttribute]
    public static void ThrowArgumentException(string message, string arg) => throw new ArgumentException(message, arg);
    [DoesNotReturnAttribute]
    public static void ThrowWhenCreatedWithNull() => throw new ArgumentException("Cannot create a value object with null.");
    [DoesNotReturnAttribute]
    public static void ThrowWhenNotInitialized() => throw new ArgumentException("Use of uninitialized Value Object.");
    [DoesNotReturnAttribute]
    public static void ThrowWhenNotInitialized(StackTrace? stackTrace) => throw new ArgumentException("Use of uninitialized Value Object at: " + stackTrace);
    [DoesNotReturnAttribute]
    public static void ThrowWhenValidationFails(Validation validation)
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
    public static System.Exception CreateCannotBeNullException() => new ValueObjectValidationException("Cannot create a value object with null.");

    public static System.Exception CreateValidationException(string message) => new ValueObjectValidationException(message);
    public static System.Exception CreateValidationException(Validation validation)
    {
        var ex = CreateValidationException(validation.ErrorMessage);
        if (validation.Data is not null)
        {
            foreach (var kvp in validation.Data)
            {
                ex.Data[kvp.Key] = kvp.Value;
            }
        }

        return ex;
    }
}
