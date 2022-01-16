using SharedUtils.Synchronisation.References;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Accountant.Accounts
{
    public class AccountantSession
    {
        internal ObjectReference<Account> Account { get; set; }

        internal bool IsLoggedIn { get; set; }

        internal int LogonNagCooldown;

        public bool TryGetAccount(out ObjectReference<Account> acc)
        {
            acc = null;

            var refn = Account;

            if (refn == null)
                return false;

            return refn.TryDuplicate(out acc);
        }

        internal bool TryNag()
        {
            if (LogonNagCooldown == 0)
            {
                LogonNagCooldown = (int)Math.Ceiling(AccountantPlugin.Instance.Configuration.NagCooldown * 60);

                return true;
            }

            return false;
        }

        internal void Update()
        {
            LogonNagCooldown--;
        }
    }
}
