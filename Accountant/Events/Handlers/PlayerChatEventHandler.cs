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
    class PlayerChatEventHandler : Frostspark.API.Events.EventHandler<PlayerChatEvent>
    {
        public override void Handle(PlayerChatEvent pce)
        {
            if (pce.Cancelled)
                return;

            var ply = pce.Player as Frostspark.Server.Entities.Player;

            var sesn = ply.Session();

            var accref = sesn.Account;

            if(accref != null)
            {
                lock (accref)
                {
                    if(accref.Valid)
                    {
                        return;
                    }
                }
            }

            pce.Cancelled = true;

            ply.SendErrorMessage($"You must be logged in to chat.");
        }
    }
}
