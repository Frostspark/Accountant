using Accountant.Extensions;

using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Handlers.Player
{
    public class PlayerCommandEventHandler : Frostspark.API.Events.SyncEventHandler<PlayerCommandEvent>
    {
        public override void Handle(PlayerCommandEvent pce)
        {
            if (pce.Cancelled || AccountantPlugin.Instance.Configuration.AllowGuests)
                return;

            //TODO: Keep this updated with Frostspark's command implementation, which is currently case-sensitive.

            foreach (var x in AccountantPlugin.Instance.Configuration.GuestCommands)
                if (pce.Command.StartsWith(x))
                    return;

            var player = pce.Player as Frostspark.Server.Entities.Player;

            if (!player.IsLoggedIn())
            {
                pce.Cancelled = true;

                player.SendErrorMessage("You must be logged in to use commands.");
            }
        }
    }
}
