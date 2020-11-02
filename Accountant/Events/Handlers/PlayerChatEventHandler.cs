using Accountant.Extensions;
using Frostspark.API.Events;
using Frostspark.API.Events.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Handlers
{
    public class PlayerChatEventHandler : Frostspark.API.Events.EventHandler<PlayerChatEvent>
    {
        public override void Handle(PlayerChatEvent pce)
        {
            if (pce.Cancelled || AccountantPlugin.Instance.Configuration.AllowGuests)
                return;

            var ply = pce.Player as Frostspark.Server.Entities.Player;

            var sesn = ply.Session();

            if(sesn.TryGetAccount(out var refn))
            {
                refn.Dispose();
                return;
            }

            pce.Cancelled = true;

            ply.SendErrorMessage($"You must be logged in to chat.");
        }
    }
}
