﻿using Accountant.Accounts;

using Frostspark.API.Events;
using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Handlers.Player
{
    public class PlayerConnectEventHandler : Frostspark.API.Events.SyncEventHandler<PlayerConnectEvent>
    {

        public override void Handle(PlayerConnectEvent obj)
        {
            AccountantSession session = new AccountantSession();

            obj.Player.SetMetadata(AccountantPlugin.Instance, true, "auth_session", session);
        }
    }
}
