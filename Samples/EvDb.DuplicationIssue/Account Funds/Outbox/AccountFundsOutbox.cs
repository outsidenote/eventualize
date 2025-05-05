using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funds.Withdraw.WithdrawFunds;

[EvDbAttachMessageType<FundsWithdrewMessage>]
[EvDbOutbox<AccountFundsFactory>]
internal partial class AccountFundsOutbox
{
    
}
