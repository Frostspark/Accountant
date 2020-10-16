using Accountant.Accounts;
using Accountant.Commands;
using Accountant.Configuration;

using Frostspark.Server;

using SharedUtils.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Accountant
{
    public class AccountantPlugin : Frostspark.API.Plugins.Plugin
    {
        internal static AccountantPlugin Instance { get; private set; }

        internal static Server Server => Server.Instance;

        internal CommandManager Commands;
        internal AccountantConfig Configuration;
        internal AccountManager Accounts;

        public AccountantPlugin()
        {
            Commands = new CommandManager(Server, this);
            Instance = this;
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
            ConfigManager.LoadConfig<AccountantConfig>(Path.Combine(DataFolder, "config.json"));
        }

        public override void Unload()
        {
            
        }
    }
}
