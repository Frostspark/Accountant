
using Frostspark.API;
using Frostspark.API.Entities;
using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Definitions.Players
{
    /// <summary>
    /// Fired when a player is logged out of an account.
    /// </summary>
    public class PlayerLogoutEvent : PlayerEvent
    {
        public PlayerLogoutEvent(Player player, Server server) : base(player, server)
        {
        }
    }
}
