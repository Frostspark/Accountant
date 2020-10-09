using Accountant.Accounts;
using Accountant.Extensions;

using Frostspark.Server.Entities;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Commands.Utilities
{
    internal static class SessionUtilities
    {
        internal static bool AcquireSession(Player ply, out AccountantSession session)
        {
            session = ply.Session();

            if (session == null)
            {
                ply.SendErrorMessage("An error has occured while trying to authenticate you. Please reconnect and try again.");
                AccountantPlugin.Instance.Log.LogError($"Error occured during logon for player {ply.Name} (addr {ply.RemoteAddress}): Session in memory is null.");
            }

            return session != null;
        }
    }
}
