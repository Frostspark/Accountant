
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

namespace Accountant.Commands.Implementations
{
    [CommandName("register")]
    [CommandDescription("Registers a new account with a given name.")]
    [CommandPermission("accountant.commands.register")]
    [NoParameterLogging]
    public class RegisterCommand : CommandWrapper<CommandSender>
    {
        [CommandCallback]
        public void CreateWithPassword(string password)
        {
            if (!EntityAssertions.Assert_SenderPlayer(Sender, out Player ply))
                return;

            CreateWithNameAndPassword(ply.Name, password);
        }

        [CommandCallback]
        public void CreateWithNameAndPassword(string username, string password)
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

            if (AccountUtilities.TryFindAccount(username, out var refn))
            {
                ply.SendErrorMessage($"This account is already registered.");
                refn.Dispose();
                return;
            }

            //The above may seem to warrant the account to be non-existent, but concurrent creation attempts may exist on the same database server.
            //A creation attempt will confirm whether or not the account exists.

            var result = AccountantPlugin.Instance.Accounts.CreateAccount(username, password, out refn);

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
