using Accountant.Accounts;
using Accountant.Accounts.Metadata;
using Accountant.Commands;
using Accountant.Configuration;
using Accountant.Events;
using Accountant.Storage;

using Frostspark.Server;

using SharedUtils.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Accountant
{
    public class AccountantPlugin : Frostspark.API.Plugins.Plugin
    {
        internal static AccountantPlugin Instance { get; private set; }

        internal static Server Server => Server.Instance;

        internal CommandManager Commands;
        internal EventManager Events;
        internal AccountantConfig Configuration;
        internal AccountManager Accounts;
        internal StorageProvider Provider;
        internal MetadataHolderRegistry MetadataRegistry;

        public AccountantPlugin()
        {
            Instance = this;
        }

        public override string Name => "Accountant";

        public override string Author => "quake1337";

        public override void Disable()
        {
            Commands.Deregister();
            Events.Deregister();
        }

        public override void Enable()
        {
            Commands.Register();
            Events.Register();
        }

        public override void Load()
        {
            Commands = new CommandManager(Server, this);
            Events = new EventManager(Server, this);

            StorageProvider.RegisterDefaultProviders();
            var types = StorageProvider.ConfigTypes;

            Configuration = ConfigManager.LoadConfig<AccountantConfig>(Path.Combine(DataFolder, "config.json"), new() { PolymorphicTypes = types, Indented = true });

            Provider = StorageProvider.SetupStorageProvider(Configuration.Storage);
            Provider.Initialize();

            MetadataRegistry = new MetadataHolderRegistry();
            MetadataRegistry.SetupDefaults();

            Accounts = new AccountManager(this, Provider);

            Provider.SetManager(Accounts);
        }

        public override void Unload()
        {
            Commands = null;
            Configuration = null;
            Provider = null;
            Accounts = null;
        }
    }
}
