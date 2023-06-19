using Accountant.Accounts;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Extensions
{
    internal static class AccountExtensions
    {
        internal static void Signout(this Account account)
        {
            AccountantPlugin.Server.Players.ForEach(ply =>
            {
                ply.TrySignOut(account);
            });
        }
    }
}
