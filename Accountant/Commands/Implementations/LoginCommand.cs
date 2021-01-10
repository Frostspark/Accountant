
using Accountant.Accounts;
using Accountant.Commands.Utilities;
using Accountant.Events.Definitions.Players;
using Accountant.Extensions;

using BCrypt.Net;

using Frostspark.Server.Commands.Assertions;
using Frostspark.Server.Commands.Attributes;
using Frostspark.Server.Entities;

using SharedUtils.Commands.Attributes;
using SharedUtils.Commands.Commands;
using SharedUtils.References;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using System.Text;

namespace Accountant.Commands.Implementations
{
    [CommandName("login")]
    [CommandDescription("Signs you into an existing account.")]
    [CommandPermission("accountant.commands.login")]
    public class LoginCommand : CommandWrapper<CommandSender>
    {
        [CommandCallback]
        public void LoginWithUUID()
        {
            if (!EntityAssertions.Assert_SenderPlayer(Sender, out Player ply))
                return;

            if (!SessionUtilities.AcquireSession(ply, out var session))
                return;

            if (session.TryGetAccount(out var accref))
            {
                ply.SendErrorMessage($"You are already logged in!");
                accref.Dispose();
                return;
            }

            if (!AccountantPlugin.Instance.Configuration.AllowUUIDSignin)
            {
                ply.SendErrorMessage("Logging in with UUID is disabled in this server. Please use the /login <password> or /login <username> <password> syntax.");
                return;
            }

            var uuid = ply.UUID;

            if (uuid == null || !Guid.TryParse(uuid, out var guid))
            {
                ply.SendErrorMessage("Cannot log you in due to an invalid UUID.");
                return;
            }

            //Ensure the UUID is correctly cased.
            uuid = guid.ToString();

            if(!AccountUtilities.TryFindAccount(ply.Name, out var refn))
            {
                ply.SendErrorMessage($"This account currently is not registered. Please use /register in order to claim it.");
                return;
            }

            Account acc = refn.Object;

            if (!acc.TryGetMetadata<AccountAutoLogins>(Account.AutoLoginMetaKey, out var aal) || !aal.Enabled)
            {
                ply.SendErrorMessage($"This account cannot be automatically logged into.");
                refn.Dispose();
                return;
            }

            if (!aal.UUIDs.Contains(uuid))
            {
                ply.SendErrorMessage($"You must log into this account with a password from this computer at least once to gain auto-login ability.");
                refn.Dispose();
                return;
            }

            PerformLogon(ply, session, refn);
        }

        [CommandCallback]
        public void LoginWithPassword(string password)
        {
            if (!EntityAssertions.Assert_SenderPlayer(Sender, out Player ply))
                return;

            if (!SessionUtilities.AcquireSession(ply, out var session))
                return;

            if (session.TryGetAccount(out var accref))
            {
                ply.SendErrorMessage($"You are already logged in!");
                accref.Dispose();
                return;
            }

            if (!AccountUtilities.TryFindAccount(ply.Name, out var refn))
            {
                ply.SendErrorMessage($"This account is not currently registered. Please use /register in order to claim it.");
                return;
            }

            Account acc = refn.Object;

            if (!BCrypt.Net.BCrypt.EnhancedVerify(password, acc.Password))
            {
                ply.SendErrorMessage("The password you provided is invalid.");
                refn.Dispose();
                return;
            }
            else
            {
                PerformLogon(ply, session, refn);
            }
        }

        [CommandCallback]
        public void LoginWithUsernamePassword(string username, string password)
        {
            if (!EntityAssertions.Assert_SenderPlayer(Sender, out Player ply))
                return;

            if (!SessionUtilities.AcquireSession(ply, out var session))
            {
                return;
            }

            if (session.TryGetAccount(out var accref))
            {
                ply.SendErrorMessage($"You are already logged in!");
                accref.Dispose();
                return;
            }

            if (!AccountUtilities.TryFindAccount(username, out var refn))
            {
                ply.SendErrorMessage($"This account is not currently registered. Please use /register in order to claim it.");
                return;
            }

            Account acc = refn.Object;

            if (!BCrypt.Net.BCrypt.EnhancedVerify(password, acc.Password))
            {
                ply.SendErrorMessage($"The password you provided is invalid.");
                refn.Dispose();
                return;
            }
            else
            {
                PlayerLoginEvent ple = new PlayerLoginEvent(ply, AccountantPlugin.Server, acc);
                AccountantPlugin.Server.Events.FireEvent(ple);

                if (!ple.Cancelled)
                {
                    PerformLogon(ply, session, refn);
                }
                else
                {
                    ply.SendErrorMessage($"Account logon denied by another plugin.");
                }
            }

        }

        private void PerformLogon(Player player, AccountantSession session, ObjectReference<Account> refn)
        {
            var acc = refn.Object;

            session.Account = refn;
            session.IsLoggedIn = true;
            player.SendSuccessMessage($"Logged in as {acc.Username} successfully.");
            acc.UpdateLogonTime();
        }
    }
}
