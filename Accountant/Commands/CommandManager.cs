using Accountant.Commands.Implementations;

using Frostspark.Server;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Commands
{
    internal class CommandManager
    {
        private readonly Server Server;
        private readonly AccountantPlugin Plugin;

        public CommandManager(Server server, AccountantPlugin plugin)
        {
            Server = server;
            Plugin = plugin;
        }

        internal void Register()
        {
            var cmdmgr = Server.Commands;

            Server.Log.LogDebug($"[Accountant] EndpointMode: {Plugin.Configuration.EndpointMode}");

            if (!Plugin.Configuration.EndpointMode)
            {
                cmdmgr.RegisterCommand<LoginCommand>();
                cmdmgr.RegisterCommand<RegisterCommand>();
                cmdmgr.RegisterCommand<ChangePasswordCommand>();
                cmdmgr.RegisterCommand<AccountantCommand>();
                cmdmgr.RegisterCommand<LogoutCommand>();
            }

        }

        internal void Deregister()
        {
            var cmdmgr = Server.Commands;

            if (!Plugin.Configuration.EndpointMode)
            {
                cmdmgr.DeregisterCommand<LoginCommand>();
                cmdmgr.DeregisterCommand<RegisterCommand>();
                cmdmgr.DeregisterCommand<ChangePasswordCommand>();
                cmdmgr.DeregisterCommand<AccountantCommand>();
                cmdmgr.DeregisterCommand<LogoutCommand>();
            }
        }
    }
}
