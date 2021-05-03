using Accountant.Events.Definitions.Players;
using Accountant.Extensions;

using Frostspark.API.Events.Interfaces;
using Frostspark.API.Events.Players;
using Frostspark.API.Events.Server;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Handlers.Player
{
    public class PlayerTargetedEventHandler : Frostspark.API.Events.SyncEventHandler<IHasTarget<Frostspark.API.Entities.Player>>
    {
        private static Type[] DontHandle = new[] { typeof(PlayerConnectEvent), typeof(PlayerDisconnectEvent), typeof(PlayerUpdateEvent), typeof(PlayerLoginEvent), typeof(PlayerLogoutEvent) };

        public override bool Filter(IHasTarget<Frostspark.API.Entities.Player> t)
        {
            return EventManager.PlayerImmobiliseEventFilter(t.Target);
        }

        public override void Handle(IHasTarget<Frostspark.API.Entities.Player> obj)
        {
            var fsplayer = obj.Target as Frostspark.Server.Entities.Player;

            //Don't lock the player up when they're logged in.
            if (fsplayer.IsLoggedIn())
                return;

            //If this is an event we're not supposed to handle, don't handle it.
            if (DontHandle.Contains(obj.GetType()))
                return;

            //Otherwise, if this event is cancellable, cancel it.
            if (obj is ICancellable cancellable)
            {
                cancellable.Cancelled = true;
            }
        }
    }
}
