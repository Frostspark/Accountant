using Accountant.Accounts;
using Accountant.API;
using Accountant.Commands;
using Accountant.Configuration;
using Accountant.Events;
using Accountant.Storage;

using Frostspark.Server;

using SharedUtils.Configuration;
using SharedUtils.Configuration.Metadata;
using SharedUtils.References;
using SharedUtils.Storage.Exceptions;

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

        public AccountantAPI API { get; private set; }

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

            Configuration = ConfigManager.LoadConfig<AccountantConfig>(Path.Combine(DataFolder, "config.json"), new ConfigSettings() { PolymorphicTypes = types, Indented = true });

            SetStorageProvider(StorageProvider.SetupFromConfig(Configuration.Storage));

            MetadataRegistry = new MetadataHolderRegistry();

            Accounts = new AccountManager(this);

            Provider.SetManager(Accounts);

            API = new AccountantAPI(this);
        }

        public void SetStorageProvider(StorageProvider provider)
        {
            Provider = provider;
            provider.Initialize();
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
