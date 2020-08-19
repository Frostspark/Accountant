using Accountant.Commands;

using Frostspark.Server;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant
{
    public class AccountantPlugin : Frostspark.API.Plugins.Plugin
    {
        internal static Server Server => Server.Instance;
        internal CommandManager Commands;

        public AccountantPlugin()
        {
            Commands = new CommandManager(Server, this);
        }

        public override string Name => "Accountant";

        public override string Author => "quake1337";

        public override void Disable()
        {
            Commands.Deregister();
        }

        public override void Enable()
        {
            Commands.Register();
        }

        public override void Load()
        {
            
        }

        public override void Unload()
        {
            
        }
    }
}
