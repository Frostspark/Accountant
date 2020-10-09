using Accountant.Accounts;
using Accountant.Extensions;

using Frostspark.API.Events;
using Frostspark.API.Events.Players;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Events.Handlers
{
    public class PlayerDisconnectEventHandler : Frostspark.API.Events.EventHandler<PlayerDisconnectEvent>
    {
        public override void Handle(PlayerDisconnectEvent obj)
        {
            var ply = obj.Player as Frostspark.Server.Entities.Player;

            var sess = ply.Session();

            if(sess == null)
            {
                return;
            }

            var accref = sess.Account;

            if(accref != null)
            {
                sess.Account = null;

                if(accref.Valid)
                {
                    accref.Dispose();
                }
            }
        }
    }
}
