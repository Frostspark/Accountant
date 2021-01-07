using Accountant.Accounts;
using Accountant.Extensions;

using Frostspark.API.Events;
using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Handlers.Player
{
    public class PlayerDisconnectEventHandler : Frostspark.API.Events.EventHandler<PlayerDisconnectEvent>
    {
        public override void Handle(PlayerDisconnectEvent obj)
        {
            var ply = obj.Player as Frostspark.Server.Entities.Player;

            ply.Signout();
        }
    }
}
