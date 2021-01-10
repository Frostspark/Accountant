using Accountant.Extensions;

using Frostspark.API.Events.Interfaces;
using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Handlers.Player
{
    public class PlayerEventHandler : Frostspark.API.Events.EventHandler<PlayerEvent>
    {
        private static Type[] DontHandle = new[] { typeof(PlayerConnectEvent), typeof(PlayerDisconnectEvent), typeof(PlayerCommandEvent), typeof(PlayerUpdateEvent) };

        public override void Handle(PlayerEvent obj)
        {
            var fsplayer = obj.Player as Frostspark.Server.Entities.Player;

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
