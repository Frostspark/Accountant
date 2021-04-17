
using Accountant.Accounts;
using Accountant.Accounts.Enums;
using Accountant.Commands.Utilities;

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
    [CommandName("accountant")]
    [CommandDescription("Plugin and account management commands for server administrators")]
    [CommandPermission("accountant.commands.accountant")]
    public class AccountantCommand : CommandWrapper<CommandSender>
    {
        [SubCommand("remove")]
        [CommandPermission("accountant.commands.accountant.remove")]
        public async Task DeleteAccount(string username)
        {
            var (result, reference) = await AccountUtilities.TryFindAccount(username);

            if (result != FindAccountResult.Found)
            {
                switch (result)
                {
                    case FindAccountResult.NotFound:
                        Sender.SendErrorMessage("An account by this name could not be found.");
                        break;
                    case FindAccountResult.Error:
                        Sender.SendErrorMessage("Account deletion failed due to a server error.");
                        break;
                }
                return;
            }

            var del_result = await AccountantPlugin.Instance.Accounts.DeleteAccount(username);

            reference.Dispose();

            switch (del_result)
            {
                case AccountDeleteResult.Success:
                    Sender.SendSuccessMessage($"Account {username} has been successfully deleted.");
                    break;
                case AccountDeleteResult.NotFound:
                    Sender.SendErrorMessage("An account by this name could not be found.");
                    break;
                case AccountDeleteResult.PluginBlocked:
                    Sender.SendErrorMessage("Account deletion blocked by another plugin.");
                    break;
                case AccountDeleteResult.StorageError:
                    Sender.SendErrorMessage("Account deletion failed due to a server error.");
                    break;
            }
        }
    }
}
