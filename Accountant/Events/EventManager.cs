using Accountant.Events.Handlers;
using Frostspark.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events
{
    internal class EventManager
    {
        private Server Server;
        private AccountantPlugin Plugin;

        private PlayerConnectEventHandler ConnectEventHandler = new PlayerConnectEventHandler();
        private PlayerDisconnectEventHandler DisconnectEventHandler = new PlayerDisconnectEventHandler();

        internal EventManager(Server server, AccountantPlugin plugin)
        {
            Server = server;
            Plugin = plugin;
        }

        internal void Register()
        {
            Server.Events.RegisterHandler(Plugin, ConnectEventHandler);
            Server.Events.RegisterHandler(Plugin, DisconnectEventHandler);
        }

        internal void Deregister()
        {
            Server.Events.UnregisterHandler(Plugin, ConnectEventHandler);
            Server.Events.UnregisterHandler(Plugin, DisconnectEventHandler);
        }
    }
}
