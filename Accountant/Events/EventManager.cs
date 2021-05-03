using Accountant.Events.Handlers.Player;
using Accountant.Events.Handlers.Server;
using Accountant.Extensions;

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
        private ServerPlayerCommandEventHandler CommandEventHandler = new ServerPlayerCommandEventHandler();
        private PlayerUpdateEventHandler UpdateEventHandler = new PlayerUpdateEventHandler();
        private PlayerTargetedEventHandler TargetedWildcardEventHandler = new PlayerTargetedEventHandler();
        private PlayerSourcedEventHandler SourcedWildcardEventHandler = new PlayerSourcedEventHandler();

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
            Server.Events.RegisterHandler(Plugin, TargetedWildcardEventHandler);
            Server.Events.RegisterHandler(Plugin, SourcedWildcardEventHandler);
        }

        internal void Deregister()
        {
            Server.Events.UnregisterHandler(Plugin, ConnectEventHandler);
            Server.Events.UnregisterHandler(Plugin, DisconnectEventHandler);
            Server.Events.UnregisterHandler(Plugin, CommandEventHandler);
            Server.Events.UnregisterHandler(Plugin, UpdateEventHandler);
            Server.Events.UnregisterHandler(Plugin, TargetedWildcardEventHandler);
            Server.Events.UnregisterHandler(Plugin, SourcedWildcardEventHandler);
        }

        internal static bool PlayerImmobiliseEventFilter(Frostspark.API.Entities.Player p)
        {
            var fsplayer = p as Frostspark.Server.Entities.Player;

            //Don't lock the player up when they're logged in.
            if (fsplayer.IsLoggedIn())
                return false;

            return true;
        }
    }
}
