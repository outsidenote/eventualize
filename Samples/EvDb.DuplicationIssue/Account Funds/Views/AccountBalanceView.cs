using Funds.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funds.Withdraw.WithdrawFunds;

[EvDbViewType<ImmutableDictionary<Currency, AccountBalanceState>, IAccountFundsEvents>("withdrawals-in-inprocess")]
internal partial class AccountBalanceView
{
    protected override ImmutableDictionary<Currency, AccountBalanceState> DefaultState { get; } = ImmutableDictionary<Currency, AccountBalanceState>.Empty;

    public override int MinEventsBetweenSnapshots { get; } = 10;

    protected override ImmutableDictionary<Currency, AccountBalanceState> Fold(ImmutableDictionary<Currency, AccountBalanceState> state, FundsWithdrawnFromAccountEvent payload, IEvDbEventMeta meta)
    {
        return base.Fold(state, payload, meta);
    }

}
