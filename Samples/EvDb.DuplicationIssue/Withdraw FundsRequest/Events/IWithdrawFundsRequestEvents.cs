using EvDb.Core;

namespace Funds.Withdraw.WithdrawFunds;

[EvDbAttachEventType<FundsWithdrawalApprovedEvent>]
[EvDbAttachEventType<FundsWithdrawalDeclinedEvent>]
public partial interface IWithdrawFundsRequestEvents
{
}
