using Accountant.Accounts;
using Accountant.Events.Definitions.Players;

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

        /// <summary>
        /// Signs this player out of their account.
        /// <para>If <paramref name="out_of"/> is not null, the player is signed out only if they're signed into the account supplied.</para>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="out_of"></param>
        /// <returns></returns>
        internal static bool Signout(this Player player, Account out_of = null)
        {
            var session = player.Session();

            if (session == null)
                return false;

            var accref = session.Account;

            if (accref?.Valid ?? false)
            {
                lock (accref)
                {
                    var acc = accref.Object;

                    if (out_of == null || out_of == acc)
                    {
                        PlayerLogoutEvent ple = new PlayerLogoutEvent(player, AccountantPlugin.Server);

                        AccountantPlugin.Server.Events.FireEvent(ple);

                        accref.Dispose();
                        session.Account = null;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
