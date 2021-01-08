using Accountant.Accounts;

using Frostspark.API;
using Frostspark.API.Events;
using Frostspark.API.Events.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Definitions.Accounts
{
    /// <summary>
    /// Raised when account is being deleted, before it's dropped from storage.
    /// </summary>
    public class AccountDeleteEvent : AccountEvent, ICancellable
    {
        internal AccountDeleteEvent(Account account, Server server) : base(account, server)
        {
        }

        public bool Cancelled { get; set; }
    }
}
