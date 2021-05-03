using Accountant.Extensions;

using Frostspark.API.Events.Players;
using Frostspark.API.Events.Server;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Handlers.Server
{
    public class ServerPlayerCommandEventHandler : Frostspark.API.Events.SyncEventHandler<ServerPlayerCommandEvent>
    {
        public override void Handle(ServerPlayerCommandEvent pce)
        {
            if (pce.Cancelled || AccountantPlugin.Instance.Configuration.AllowGuests)
                return;

            //TODO/WATCHME: Keep this updated with Frostspark's command implementation, which is currently case-sensitive.
            if (AccountantPlugin.Instance.Configuration.GuestCommands.Any(x => pce.Command.StartsWith(x)))
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
