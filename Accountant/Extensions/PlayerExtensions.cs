using Accountant.Accounts;

using Frostspark.Server.Entities;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Extensions
{
    internal static class PlayerExtensions
    {
        internal static AccountantSession Session(this Player player)
        {
            if (!player.GetMetadata(AccountantPlugin.Instance, true, "auth_session", out AccountantSession val))
                return null;

            return val;
        }
    }
}
