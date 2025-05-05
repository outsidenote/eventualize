using Vogen;

namespace Funds.Withdraw.WithdrawFunds;

#pragma warning disable S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
[ValueObject<double>(Conversions.TypeConverter | Conversions.SystemTextJson,
    toPrimitiveCasting: CastOperator.Implicit,
    fromPrimitiveCasting: CastOperator.Implicit,
    tryFromGeneration: TryFromGeneration.GenerateBoolMethod,
    isInitializedMethodGeneration: IsInitializedMethodGeneration.Generate)]
public readonly partial struct Commission
{
    private static Validation Validate(double input) => input > 1 || input < 0
                        ? Validation.Ok
                        : Validation.Invalid("Commissions range should be between 0-1");

}
