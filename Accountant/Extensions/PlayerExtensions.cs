using Accountant.Accounts;
using Accountant.Events.Definitions.Players;

using Frostspark.Server.Entities;

using SharedUtils.Synchronisation.References;

using System.Threading.Tasks;

namespace Accountant.Extensions
{
    public static class PlayerExtensions
    {
        public static AccountantSession Session(this Frostspark.API.Entities.Player player)
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
        /// <returns>Whether or not the user's authentication status changed.</returns>
        public static bool Signout(this Player player, Account out_of = null)
        {
            var session = player.Session();

            if (session == null)
                return false;

            var accref = session.Account;

            if (accref == null)
                return false;

            if (accref.TryUnpack(out var acc))
            {
                if (out_of == null || out_of == acc)
                {
                    PlayerLogoutEvent ple = new PlayerLogoutEvent(player, AccountantPlugin.Server);

                    AccountantPlugin.Server.Events.FireEvent(ple);

                    accref.Dispose();
                    session.Account = null;
                    session.IsLoggedIn = false;

                    return true;
                }
            }

            return false;
        }

        internal static bool IsLoggedIn(this Player player)
        {
            var session = player.Session();

            if (session == null)
                return false;

            return session.IsLoggedIn;
        }

        public static async Task TryPerformLogon(this Player player, ObjectReference<Account> refn)
        {
            var session = player.Session();

            if (!refn.TryUnpack(out var acc))
            {
                //Generic failure.
                refn.Dispose();
                return;
            }

            PlayerLoginEvent ple = new(player, AccountantPlugin.Server, acc);

            await AccountantPlugin.Server.Events.FireEventAsync(ple);

            if (ple.Cancelled)
            {
                player.SendErrorMessage($"Account logon denied by another plugin.");
                refn.Dispose();
            }
            else
            {
                session.Account = refn;
                session.IsLoggedIn = true;
                player.SendSuccessMessage($"Logged in as {acc.Username} successfully.");
                await acc.UpdateLogonTime();
            }
        }
    }
}
