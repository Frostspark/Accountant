using SharedUtils.References;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Accounts
{
    public class AccountantSession
    {
        internal ObjectReference<Account> Account { get; set; }

        public bool TryGetAccount(out ObjectReference<Account> acc)
        {
            acc = null;

            var refn = Account;

            if (refn == null)
                return false;

            lock (refn)
            {
                if (!refn.Valid)
                    return false;

                acc = refn.Duplicate();
                return true;
            }
        }
    }
}
