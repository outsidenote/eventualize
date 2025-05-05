namespace Funds.Withdraw.WithdrawFunds;

/// <summary>
/// Withdraw Funds Request factory.
/// </summary>
[EvDbAttachView<AccountBalanceView>]
[EvDbStreamFactory<IAccountFundsEvents, AccountFundsOutbox>("Funds.Withdraw", "Account.Funds")]
public partial class AccountFundsFactory
{
}
