using Accountant.Extensions;

using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Handlers.Player
{
    public class PlayerUpdateEventHandler : Frostspark.API.Events.SyncEventHandler<PlayerUpdateEvent>
    {
        public override void Handle(PlayerUpdateEvent pue)
        {
            var ply = pue.Player as Frostspark.Server.Entities.Player;

            var sesn = ply.Session();

            sesn.Update();
        }
    }
}
