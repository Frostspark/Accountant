using Accountant.Events.Definitions.Players;
using Accountant.Extensions;

using Frostspark.API.Events.Interfaces;
using Frostspark.API.Events.Players;
using Frostspark.API.Events.Server;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Handlers.Player
{
    public class PlayerSourcedEventHandler : Frostspark.API.Events.SyncEventHandler<IHasSource<Frostspark.API.Entities.Player>>
    {
        private static Type[] DontHandle = new Type[] { };

        public override bool Filter(IHasSource<Frostspark.API.Entities.Player> t)
        {
            return EventManager.PlayerImmobiliseEventFilter(t.Source);
        }

        public override void Handle(IHasSource<Frostspark.API.Entities.Player> obj)
        {
            //If this is an event we're not supposed to handle, don't handle it.
            if (DontHandle.Contains(obj.GetType()))
                return;

            //Otherwise, if this event is cancellable, cancel it.
            if (obj is ICancellable cancellable)
            {
                cancellable.Cancelled = true;
            }
        }
    }
}
