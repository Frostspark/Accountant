
using Accountant.Accounts;

using Frostspark.API;
using Frostspark.API.Entities;
using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Definitions.Players
{
    /// <summary>
    /// Fired when a player is logged into an account.
    /// </summary>
    public class PlayerLoginEvent : PlayerEvent
    {
        public PlayerLoginEvent(Player player, Server server) : base(player, server)
        {
        }

        /// <summary>
        /// Account this user was logged into.
        /// <para>Do not store this object directly, </para>
        /// </summary>
        public Account Account { get; }
    }
}
