using Accountant.Storage;

using Frostspark.API;
using Frostspark.API.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Events.Definitions.Storage
{
    public class StorageProviderSetupEvent : Event
    {
        public StorageProviderSetupEvent(Server server) : base(server)
        {
        }

        public StorageProvider Provider { get; set; }
    }
}
