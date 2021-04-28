
using Accountant.Accounts;
using Accountant.Accounts.Enums;
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
using System.Threading.Tasks;

namespace Accountant.Commands.Implementations
{
    [CommandName("login")]
    [CommandDescription("Signs you into an existing account.")]
    [CommandPermission("accountant.commands.login")]
    [NoParameterLogging]
    public class LoginCommand : CommandWrapper<CommandSender>
    {
        [CommandCallback]
        public async Task LoginWithUUID()
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

            //Ensure the UUID is correctly formatted.
            uuid = guid.ToString();

            var (result, refn) = await AccountUtilities.TryFindAccount(ply.Name);

            if (result != FindAccountResult.Found)
            {
                switch (result)
                {
                    case FindAccountResult.NotFound:
                        ply.SendErrorMessage($"This account currently is not registered. Please use /register in order to claim it.");
                        break;
                    case FindAccountResult.Error:
                        ply.SendErrorMessage("An internal server has occured while looking up this account.");
                        break;
                }
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

            await TryPerformLogon(ply, session, refn);
        }

        [CommandCallback]
        public async Task LoginWithPassword(string password)
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

            var (result, refn) = await AccountUtilities.TryFindAccount(ply.Name);

            if (result != FindAccountResult.Found)
            {
                switch (result)
                {
                    case FindAccountResult.NotFound:
                        ply.SendErrorMessage($"This account currently is not registered. Please use /register in order to claim it.");
                        break;
                    case FindAccountResult.Error:
                        ply.SendErrorMessage("An internal server has occured while looking up this account.");
                        break;
                }
                return;
            }

            Account acc = refn.Object;

            //TODO: Put on another thread.
            if (!BCrypt.Net.BCrypt.EnhancedVerify(password, acc.Password))
            {
                ply.SendErrorMessage("The password you provided is invalid.");
                refn.Dispose();
                return;
            }
            else
            {
                await TryPerformLogon(ply, session, refn);
            }
        }

        [CommandCallback]
        public async Task LoginWithUsernamePassword(string username, string password)
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

            var (result, refn) = await AccountUtilities.TryFindAccount(ply.Name);

            if (result != FindAccountResult.Found)
            {
                switch (result)
                {
                    case FindAccountResult.NotFound:
                        ply.SendErrorMessage($"This account currently is not registered. Please use /register in order to claim it.");
                        break;
                    case FindAccountResult.Error:
                        ply.SendErrorMessage("An internal server has occured while looking up this account.");
                        break;
                }
                return;
            }

            Account acc = refn.Object;

            //TODO: Put on another thread.
            if (!BCrypt.Net.BCrypt.EnhancedVerify(password, acc.Password))
            {
                ply.SendErrorMessage($"The password you provided is invalid.");
                refn.Dispose();
                return;
            }
            else
            {
                await TryPerformLogon(ply, session, refn);
            }

        }

        private static async Task TryPerformLogon(Player player, AccountantSession session, ObjectReference<Account> refn)
        {
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
