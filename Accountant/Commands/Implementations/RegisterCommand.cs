
using Accountant.Accounts;
using Accountant.Accounts.Enums;
using Accountant.Commands.Utilities;
using Accountant.Extensions;

using Frostspark.API.Entities.Interfaces;
using Frostspark.Server.Commands.Assertions;
using Frostspark.Server.Commands.Attributes;
using Frostspark.Server.Entities;

using SharedUtils.Commands.Attributes;
using SharedUtils.Commands.Commands;
using SharedUtils.References;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Commands.Implementations
{
    [CommandName("register")]
    [CommandDescription("Registers a new account with a given name.")]
    [CommandPermission("accountant.commands.register")]
    [NoParameterLogging]
    public class RegisterCommand : CommandWrapper<CommandSender>
    {
        [CommandCallback]
        public async Task CreateWithPassword(string password)
        {
            if (!EntityAssertions.Assert_SenderPlayer(Sender, out Player ply))
                return;

            await CreateWithNameAndPassword(ply.Name, password);
        }

        [CommandCallback]
        public async Task CreateWithNameAndPassword(string username, string password)
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

            //TODO: Determine if this brings any benefit when uncommented.

            //var (findresult, xrefn) = await AccountUtilities.TryFindAccount(username);

            //if (findresult == FindAccountResult.Found)
            //{
            //    ply.SendErrorMessage($"This account is already registered.");
            //    xrefn.Dispose();
            //    return;
            //}

            //The above attempts to short-circuit when an account is verifiably present in memory.
            //A creation attempt will confirm whether or not the account exists in storage.

            (AccountCreateResult result, ObjectReference<Account> refn) = await AccountantPlugin.Instance.Accounts.CreateAccount(username, password);

            switch(result)
            {
                case AccountCreateResult.Success:
                    session.Account = refn;
                    ply.SendSuccessMessage($"The account {username} was successfully created, and you have been logged in.");
                    break;
                case AccountCreateResult.AlreadyExists:
                    ply.SendErrorMessage($"This account is already registered.");
                    break;
                case AccountCreateResult.PluginBlocked:
                    ply.SendErrorMessage($"Account creation denied by another plugin.");
                    break;
                case AccountCreateResult.StorageError:
                    ply.SendErrorMessage($"Account creation failed due to a database error. Try again later.");
                    break;
            }
        }
    }
}
