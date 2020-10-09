using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Accounts
{
    internal class AccountAutoLogins
    {
        internal AccountAutoLogins()
        {

        }

        internal bool Enabled;
        internal long Account;
        internal List<string> UUIDs;
    }
}
