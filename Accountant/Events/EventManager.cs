using Accountant.Events.Handlers.Player;

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
        private PlayerCommandEventHandler CommandEventHandler = new PlayerCommandEventHandler();
        private PlayerUpdateEventHandler UpdateEventHandler = new PlayerUpdateEventHandler();
        private PlayerEventHandler EventHandler = new PlayerEventHandler();

        internal EventManager(Server server, AccountantPlugin plugin)
        {
            Server = server;
            Plugin = plugin;
        }

        internal void Register()
        {
            Server.Events.RegisterHandler(Plugin, ConnectEventHandler);
            Server.Events.RegisterHandler(Plugin, DisconnectEventHandler);
            Server.Events.RegisterHandler(Plugin, CommandEventHandler);
            Server.Events.RegisterHandler(Plugin, UpdateEventHandler);
            Server.Events.RegisterHandler(Plugin, EventHandler);
        }

        internal void Deregister()
        {
            Server.Events.UnregisterHandler(Plugin, ConnectEventHandler);
            Server.Events.UnregisterHandler(Plugin, DisconnectEventHandler);
            Server.Events.UnregisterHandler(Plugin, CommandEventHandler);
            Server.Events.UnregisterHandler(Plugin, UpdateEventHandler);
            Server.Events.UnregisterHandler(Plugin, EventHandler);
        }
    }
}
