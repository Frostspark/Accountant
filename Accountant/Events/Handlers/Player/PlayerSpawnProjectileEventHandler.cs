using Accountant.Extensions;

using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Handlers.Player
{
    public class PlayerSpawnProjectileEventHandler : Frostspark.API.Events.EventHandler<PlayerSpawnProjectileEvent>
    {
        public override void Handle(PlayerSpawnProjectileEvent pspe)
        {
            if (pspe.Cancelled || AccountantPlugin.Instance.Configuration.AllowGuests)
                return;

            var ply = pspe.Player as Frostspark.Server.Entities.Player;

            var sesn = ply.Session();

            if (sesn.TryGetAccount(out var refn))
            {
                refn.Dispose();
                return;
            }

            pspe.Cancelled = true;

            if (sesn.TryNag())
                ply.SendErrorMessage($"You must be logged in to spawn projectiles.");
        }
    }
}
