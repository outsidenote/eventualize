using EvDb.Core;

namespace Funds.Withdraw.WithdrawFunds;

[EvDbAttachEventType<FundsWithdrawnFromAccountEvent>]
public partial interface IAccountFundsEvents
{
}
