
using Accountant.Accounts;
using Accountant.Extensions;

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
    internal class RegisterCommand : CommandWrapper<CommandSender>
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

            if (session.Authenticated)
            {
                ply.SendErrorMessage($"You are already logged in!");
                return;
            }

            if (FindAccount(username, out var refn))
            {
                ply.SendErrorMessage($"This account is already registered. If it's your account, please use /login <password> to sign in.");
                refn.Dispose();
                return;
            }

            //The above may seem to warrant the account to be non-existent, but concurrent creation attempts may exist on the same database server.
            //A creation attempt will confirm whether or not the account exists.

            if(!AccountantPlugin.Instance.Accounts.CreateAccount(username, password, out refn))
            {
                if (FindAccount(username, out var refn2))
                {
                    ply.SendErrorMessage($"This account is already registered. If it's your account, please use /login <password> to sign in.");
                    refn2.Dispose();
                    return;
                }
                else
                {
                    ply.SendErrorMessage($"Account creation failed due to a database error. Try again later.");
                    return;
                }
            }
            else
            {
                session.Account = refn;
                ply.SendSuccessMessage($"The account {username} was successfully created, and you have been logged in.");
            }
        }

        private bool FindAccount(string name, out ObjectReference<Account> refn)
        {
            refn = null;

            try
            {
                refn = AccountantPlugin.Instance.Accounts.GetAccountByUsername(name);

                return true;
            }
            catch (EntryNotFoundException)
            {
                return false;
            }
        }
    }
}
