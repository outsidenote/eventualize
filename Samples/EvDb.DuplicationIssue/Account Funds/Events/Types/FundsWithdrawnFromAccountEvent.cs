using Funds.Abstractions;

namespace Funds.Withdraw.WithdrawFunds;

/// <summary>
/// Funds withdrawal approved event
/// </summary>
/// <param name="AccountId">Account identifier</param>
/// <param name="Data">Common transaction data</param>
/// <param name="InitiateMethod">The method of initiating the funds operation (like ATM, Teller, PayPal, etc.)</param>
/// <param name="Commission">The commission taken percent (0-1)</param>
[EvDbDefineEventPayload("funds-withdrawn-from-account")]
public readonly partial record struct FundsWithdrawnFromAccountEvent(AccountId AccountId,
                                              FundsTransactionData Data,
                                              FundsInitiateMethod InitiateMethod,
                                              Commission Commission);
