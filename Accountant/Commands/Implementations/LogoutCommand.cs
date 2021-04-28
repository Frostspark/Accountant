using Accountant.Commands.Utilities;
using Accountant.Extensions;

using Frostspark.Server;
using Frostspark.Server.Commands.Assertions;
using Frostspark.Server.Commands.Attributes;
using Frostspark.Server.Entities;

using SharedUtils.Commands.Attributes;
using SharedUtils.Commands.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Commands.Implementations
{
    [CommandName("logout")]
    [CommandDescription("Signs you out of the account you're logged into.")]
    [CommandPermission("accountant.commands.logout")]
    public class LogoutCommand : CommandWrapper<CommandSender>
    {
        [CommandCallback]
        public void Logout()
        {
            if (!EntityAssertions.Assert_SenderPlayer(Sender, out Player ply))
                return;

            if (!SessionUtilities.AcquireSession(ply, out var session))
                return;

            if (ply.Signout())
            {
                ply.SendSuccessMessage("You have been succesfully logged out.");
            }
            else
            {
                ply.SendErrorMessage("You are not logged in!");
            }

        }
    }
}
