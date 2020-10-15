using Accountant.Accounts;

using Frostspark.API;
using Frostspark.API.Events;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Definitions.Accounts
{
    /// <summary>
    /// Raised when an account is being modified, before changes are merged into the object & storage.
    /// </summary>
    public class AccountUpdateEvent : AccountEvent, ICancellable
    {
        internal AccountUpdateEvent(Account account, Server server) : base(account, server)
        {
        }

        public bool Cancelled { get; set; }
    }
}
