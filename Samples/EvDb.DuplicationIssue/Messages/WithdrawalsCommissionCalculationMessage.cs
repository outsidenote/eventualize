using Funds.Abstractions;
using EvDb.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funds.Withdraw.WithdrawFunds;

/// <summary>
/// Withdrawals commission calculation
/// </summary>
/// <param name="AccountId">Account identifier</param>
/// <param name="Data">Common transaction data</param>
/// <param name="InitiateMethod">The method of initiating the funds operation (like ATM, Teller, PayPal, etc.)</param>
[EvDbDefineMessagePayload("withdrawals-commission-calculation")]
public readonly partial record struct WithdrawalsCommissionCalculationMessage(AccountId AccountId,
                                                      FundsTransactionData Data,
                                                      FundsInitiateMethod InitiateMethod);