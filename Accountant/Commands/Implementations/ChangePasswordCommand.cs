
using Accountant.Accounts;
using Accountant.Commands.Utilities;

using Frostspark.Server.Commands.Assertions;
using Frostspark.Server.Commands.Attributes;
using Frostspark.Server.Entities;

using SharedUtils.Commands.Attributes;
using SharedUtils.Commands.Commands;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Commands.Implementations
{
    [Command("changepw", "changepassword")]
    [CommandDescription("Changes your account's password.")]
    [CommandPermission("accountant.commands.changepw")]
    [NoParameterLogging]
    public class ChangePasswordCommand : CommandWrapper<CommandSender>
    {

        [CommandCallback]
        public async Task ChangePassword(string old_password, string new_password, string new_password_confirm)
        {
            if (!EntityAssertions.Assert_SenderPlayer(Sender, out Player ply))
                return;

            if (!SessionUtilities.AcquireSession(ply, out var session))
                return;

            if(!session.TryGetAccount(out var accountref))
            {
                ply.SendErrorMessage($"You must be logged in to change an account's password.");
                return;
            }

            using (accountref)
            {
                if (new_password != new_password_confirm)
                {
                    ply.SendErrorMessage($"The new password and the confirmation password must be the same.");
                    return;
                }

                Account account = accountref.Object;

                if (!BCrypt.Net.BCrypt.EnhancedVerify(old_password, account.Password))
                {
                    ply.SendErrorMessage($"The current password you specified is incorrect.");
                    return;
                }

                var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(new_password);

                account.Password = hash;

                if (await account.Save())
                {
                    ply.SendSuccessMessage($"Your account's password has been successfully updated.");
                }
                else
                {
                    ply.SendErrorMessage($"Your account's password was not updated due to a server error. Please try again.");
                }
            }

        }

    }
}
