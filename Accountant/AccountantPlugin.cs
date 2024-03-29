﻿using Accountant.Accounts;
using Accountant.API;
using Accountant.Commands;
using Accountant.Configuration;
using Accountant.Events;
using Accountant.Events.Definitions.Storage;
using Accountant.Storage;

using Frostspark.Server;

using SharedUtils.Configuration;
using SharedUtils.Configuration.Metadata;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override Task Disable()
        {
            Commands.Deregister();
            Events.Deregister();
            return Task.CompletedTask;
        }

        public override Task Enable()
        {
            InitialiseStorage();
            Provider.SetManager(Accounts);

            Commands.Register();
            Events.Register();
            return Task.CompletedTask;
        }

        public override Task Load()
        {
            Commands = new CommandManager(Server, this);
            Events = new EventManager(Server, this);

            StorageProvider.RegisterDefaultProviders();
            var types = StorageProvider.ConfigTypes;

            Configuration = ConfigManager.LoadConfig<AccountantConfig>(Path.Combine(DataFolder, "config.json"), new ConfigSettings() { PolymorphicTypes = types, Indented = true });

            MetadataRegistry = new MetadataHolderRegistry();

            Accounts = new AccountManager(this);

            API = new AccountantAPI(this);

            return Task.CompletedTask;
        }

        private void InitialiseStorage()
        {
            StorageProviderSetupEvent spse = new(Server);

            Server.Events.FireEvent(spse);

            if (spse.Provider != null)
            {
                SetStorageProvider(spse.Provider);
            }
            else
            {
                SetStorageProvider(StorageProvider.SetupFromConfig(Configuration.Storage));
            }
        }

        private void SetStorageProvider(StorageProvider provider)
        {
            Provider = provider;
            provider.Initialize();
        }

        public override Task Unload()
        {
            Commands = null;
            Configuration = null;
            Provider = null;
            Accounts = null;

            return Task.CompletedTask;
        }
    }
}
