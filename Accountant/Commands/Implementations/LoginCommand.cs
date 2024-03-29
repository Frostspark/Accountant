﻿
using Accountant.Accounts;
using Accountant.Accounts.Enums;
using Accountant.Commands.Utilities;
using Accountant.Extensions;

using Frostspark.Server.Commands.Assertions;
using Frostspark.Server.Commands.Attributes;
using Frostspark.Server.Entities;

using SharedUtils.Commands.Attributes;
using SharedUtils.Commands.Commands;

using System;
using System.Threading.Tasks;

namespace Accountant.Commands.Implementations
{
    [Command("login")]
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

            await ply.TrySignIn(refn);
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
                await ply.TrySignIn(refn);
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

            var (result, refn) = await AccountUtilities.TryFindAccount(username);

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
                await ply.TrySignIn(refn);
            }

        }
    }
}
