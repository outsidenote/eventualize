using Vogen;

#pragma warning disable S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"

namespace Funds.Abstractions;

/// <summary>
/// The account identifier
/// </summary>
[ValueObject<Guid>(Conversions.TypeConverter | Conversions.SystemTextJson,
    toPrimitiveCasting: CastOperator.Implicit,
    fromPrimitiveCasting: CastOperator.Implicit,
    tryFromGeneration: TryFromGeneration.GenerateBoolMethod,
    isInitializedMethodGeneration: IsInitializedMethodGeneration.Generate)]
public readonly partial struct AccountId
{
}
