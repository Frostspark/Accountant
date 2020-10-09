using SharedUtils.References;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Accounts
{
    public class AccountantSession
    {
        internal ObjectReference<Account> Account { get; set; }

        public bool Authenticated => Account != null && Account.Valid;
    }
}
