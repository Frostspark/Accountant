
using Accountant.Accounts;

using Frostspark.API;
using Frostspark.API.Entities;
using Frostspark.API.Events;
using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Definitions.Players
{
    /// <summary>
    /// Fired when a player is logged into an account.
    /// </summary>
    public class PlayerLoginEvent : PlayerEvent, ICancellable
    {
        public PlayerLoginEvent(Player player, Server server, Account account) : base(player, server)
        {
            Account = account;
        }

        /// <summary>
        /// Account this user was logged into.
        /// <para>Do not store this object on its own beyond the lifetime of your event handler - use <see cref="Account.GetReference"/> to get a reference to store.</para>
        /// </summary>
        public Account Account { get; }

        public bool Cancelled { get; set; }
    }
}
