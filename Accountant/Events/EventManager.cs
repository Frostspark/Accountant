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
        private PlayerChatEventHandler ChatEventHandler = new PlayerChatEventHandler();
        private PlayerCommandEventHandler CommandEventHandler = new PlayerCommandEventHandler();
        private PlayerMoveEventHandler MoveEventHandler = new PlayerMoveEventHandler();
        private PlayerSpawnProjectileEventHandler SpawnProjectileEventHandler = new PlayerSpawnProjectileEventHandler();
        private PlayerUpdateEventHandler UpdateEventHandler = new PlayerUpdateEventHandler();

        internal EventManager(Server server, AccountantPlugin plugin)
        {
            Server = server;
            Plugin = plugin;
        }

        internal void Register()
        {
            Server.Events.RegisterHandler(Plugin, ConnectEventHandler);
            Server.Events.RegisterHandler(Plugin, DisconnectEventHandler);
            Server.Events.RegisterHandler(Plugin, ChatEventHandler);
            Server.Events.RegisterHandler(Plugin, CommandEventHandler);
            Server.Events.RegisterHandler(Plugin, MoveEventHandler);
            Server.Events.RegisterHandler(Plugin, SpawnProjectileEventHandler);
            Server.Events.RegisterHandler(Plugin, UpdateEventHandler);
        }

        internal void Deregister()
        {
            Server.Events.UnregisterHandler(Plugin, ConnectEventHandler);
            Server.Events.UnregisterHandler(Plugin, DisconnectEventHandler);
            Server.Events.UnregisterHandler(Plugin, ChatEventHandler);
            Server.Events.UnregisterHandler(Plugin, CommandEventHandler);
            Server.Events.UnregisterHandler(Plugin, MoveEventHandler);
            Server.Events.UnregisterHandler(Plugin, SpawnProjectileEventHandler);
            Server.Events.UnregisterHandler(Plugin, UpdateEventHandler);
        }
    }
}
