using Accountant.Accounts;

using Frostspark.API;
using Frostspark.API.Events;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Definitions.Accounts
{
    /// <summary>
    /// Raised when a new account is being created, before the account is merged into storage.
    /// </summary>
    public class AccountCreateEvent : AccountEvent, ICancellable
    {
        internal AccountCreateEvent(Account account, Server server) : base(account, server)
        {
        }

        public bool Cancelled { get; set; }
    }
}
