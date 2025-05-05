using Funds.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funds.Withdraw.WithdrawFunds;


public readonly record struct WithdrawalsInProcessItem(AccountId AccountId,
                                                      FundsTransactionData Data);
