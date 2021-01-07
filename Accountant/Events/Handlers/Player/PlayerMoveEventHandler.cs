using Accountant.Extensions;

using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Handlers.Player
{
    public class PlayerMoveEventHandler : Frostspark.API.Events.EventHandler<PlayerMoveEvent>
    {
        public override void Handle(PlayerMoveEvent pme)
        {
            if (pme.Cancelled || AccountantPlugin.Instance.Configuration.AllowGuests)
                return;

            var ply = pme.Player as Frostspark.Server.Entities.Player;

            var sesn = ply.Session();

            if (sesn.TryGetAccount(out var refn))
            {
                refn.Dispose();
                return;
            }

            pme.Cancelled = true;

            if (sesn.TryNag())
                ply.SendErrorMessage($"You must be logged in to move.");
        }
    }
}
