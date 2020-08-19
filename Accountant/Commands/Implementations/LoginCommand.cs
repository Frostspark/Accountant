
using Frostspark.Server.Commands.Attributes;
using Frostspark.Server.Entities;

using SharedUtils.Commands.Attributes;
using SharedUtils.Commands.Commands;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Commands.Implementations
{
    [CommandName("login")]
    [CommandDescription("Signs you into an existing account.")]
    [CommandPermission("accountant.commands.login")]
    internal class LoginCommand : CommandWrapper<CommandSender>
    {
        public void LoginWithUUID()
        {

        }
        public void LoginWithPassword(string password)
        {

        }

        public void LoginWithUsernamePassword(string username, string password)
        {

        }
    }
}
