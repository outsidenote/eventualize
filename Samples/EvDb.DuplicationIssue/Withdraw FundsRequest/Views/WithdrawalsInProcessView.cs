using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funds.Withdraw.WithdrawFunds;

[EvDbViewType<ImmutableArray<WithdrawalsInProcessItem>, IWithdrawFundsRequestEvents>("withdrawals-in-inprocess")]
internal partial class WithdrawalsInProcessView
{
    protected override ImmutableArray<WithdrawalsInProcessItem> DefaultState { get; } = ImmutableArray<WithdrawalsInProcessItem>.Empty;

    public override int MinEventsBetweenSnapshots { get; } = 10;

    protected override ImmutableArray<WithdrawalsInProcessItem> Fold(ImmutableArray<WithdrawalsInProcessItem> state,
                                                                     FundsWithdrawalApprovedEvent payload,
                                                                     IEvDbEventMeta meta)
    {
        var item = new WithdrawalsInProcessItem(payload.AccountId, payload.Data);
        state = state.Add(item);
        return state;
    }
}
