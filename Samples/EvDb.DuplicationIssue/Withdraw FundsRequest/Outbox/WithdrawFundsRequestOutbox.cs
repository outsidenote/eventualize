using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funds.Withdraw.WithdrawFunds;

[EvDbAttachMessageType<WithdrawalsCommissionCalculationMessage>]
[EvDbOutbox<WithdrawFundsRequestFactory>]
internal partial class WithdrawFundsRequestOutbox
{
    protected override void ProduceOutboxMessages(FundsWithdrawalApprovedEvent payload,
                                                  IEvDbEventMeta meta,
                                                  EvDbWithdrawFundsRequestViews views,
                                                  WithdrawFundsRequestOutboxContext outbox)
    {
        var message = new WithdrawalsCommissionCalculationMessage
        {
            AccountId = payload.AccountId,
            Data = payload.Data,
            InitiateMethod = payload.InitiateMethod
        };
        outbox.Add(message);
    }
}
