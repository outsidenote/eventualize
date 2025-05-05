using System.Globalization;
using Vogen;

#pragma warning disable S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"

namespace Funds.Abstractions;

/// <summary>
/// The currency ISO 3 letter code
/// </summary>
[ValueObject<string>(Conversions.TypeConverter | Conversions.SystemTextJson,
    toPrimitiveCasting: CastOperator.Implicit,
    fromPrimitiveCasting: CastOperator.Implicit,
    tryFromGeneration: TryFromGeneration.GenerateBoolMethod,
    isInitializedMethodGeneration: IsInitializedMethodGeneration.Generate)]
public readonly partial struct Currency
{
    private static Validation Validate(string input) => input.Length == 3
                        ? Validation.Ok
                        : Validation.Invalid("Expecting 3 Letters ISO standard");

    private static string NormalizeInput(string input) => input.Trim().ToUpper(CultureInfo.InvariantCulture);

}
