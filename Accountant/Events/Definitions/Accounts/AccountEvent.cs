using Accountant.Accounts;

using Frostspark.API.Events;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Definitions.Accounts
{
    public class AccountEvent : Event
    {
        internal AccountEvent(Account account, Frostspark.API.Server server) : base(server)
        {
            Account = account;
        }

        /// <summary>
        /// The account involved with this event.
        /// </summary>
        public Account Account { get; }
    }
}
