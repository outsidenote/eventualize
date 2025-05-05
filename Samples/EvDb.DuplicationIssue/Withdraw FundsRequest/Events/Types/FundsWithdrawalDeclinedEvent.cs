using Funds.Abstractions;

namespace Funds.Withdraw.WithdrawFunds;

/// <summary>
/// Funds withdrawal approved denied event
/// </summary>
/// <param name="AccountId">Account identifier</param>
/// <param name="Data">Common transaction data</param>
/// <param name="InitiateMethod">The method of initiating the funds operation (like ATM, Teller, PayPal, etc.)</param>
[EvDbDefineEventPayload("funds-withdrawal-declined")]
public readonly partial record struct FundsWithdrawalDeclinedEvent(AccountId AccountId,
                                              FundsTransactionData Data,
                                              FundsInitiateMethod InitiateMethod);