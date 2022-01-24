using Accountant.Accounts;
using Accountant.Configuration.Storage;

using Frostspark.API.Logging;

using SharedUtils.Storage;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Accountant.Storage
{
    public abstract class StorageProvider
    {
        public delegate StorageProvider StorageProviderConfigurator(StorageConfig config);
        private static readonly Dictionary<Type, StorageProviderConfigurator> Providers = new Dictionary<Type, StorageProviderConfigurator>();

        protected readonly AccountantPlugin Plugin;

        protected AccountManager Manager { get; private set; }

        protected StorageProvider(AccountantPlugin plugin, StorageConfig config)
        {
            Plugin = plugin;
        }

        internal void SetManager(AccountManager inst)
        {
            Manager = inst;
        }

        /// <summary>
        /// Initializes the provider.
        /// <para>Test for success using <see cref="StorageResult.Success"/></para>
        /// </summary>
        public abstract ValueTask<StorageResult> Initialize();

        /// <summary>
        /// Deinitialises the provider.
        /// </summary>
        /// <returns></returns>
        public abstract ValueTask<StorageResult> Deinitialise();

        /// <summary>
        /// Deletes an account.
        /// <para>Test for success using <see cref="StorageResult.Success"/></para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract ValueTask<StorageResult> DeleteAccount(Account acc);

        /// <summary>
        /// Retrieves an account by id, or throws an exception.
        /// <para>Retrieve object with <see cref="StorageResult.Object{Account}"/> where T = <see cref="Account"/></para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract ValueTask<StorageResult> GetAccount(long id);

        /// <summary>
        /// Retrieves an account by username, or throws an exception.
        /// <para>Retrieve object with <see cref="StorageResult.Object{Account}"/> where T = <see cref="Account"/></para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract ValueTask<StorageResult> GetAccount(string name);

        /// <summary>
        /// Saves an account.
        /// <para>Test for success using <see cref="StorageResult.Success"/></para>
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public abstract ValueTask<StorageResult> SaveAccount(Account account);

        /// <summary>
        /// Retrieves auto-login entries for a given uuid.
        /// <para>Retrieve object with <see cref="StorageResult.Object{AutoLoginInfo}"/> where T = <see cref="PlayerAutoLogins"/></para>
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public abstract ValueTask<StorageResult> GetAutologinEntries(string uuid);

        internal static void RegisterStorageProvider<SC>(StorageProviderConfigurator config) where SC : StorageConfig
        {
            var type = typeof(SC);

            lock (Providers)
            {
                Providers.TryAdd(type, config);
            }
        }

        internal static void DeregisterStorageProvider<SC>()
        {
            var type = typeof(SC);

            lock (Providers)
            {
                Providers.Remove(type);
            }
        }

        internal static List<Type> ConfigTypes
        {
            get
            {
                List<Type> types = new List<Type>();

                lock (Providers)
                {
                    foreach (var x in Providers)
                    {
                        types.Add(x.Key);
                    }
                }

                return types;
            }
        }

        internal static StorageProvider SetupFromConfig(StorageConfig config)
        {
            var type = config.GetType();

            lock (Providers)
            {
                if (!Providers.TryGetValue(type, out var configurator))
                {
                    AccountantPlugin.Instance.Log.LogError("Config", $"Configuration type {type.Name} isn't mapped to a storage provider.");
                    throw new InvalidOperationException("Unknown storage provider.");
                }
                else
                {
                    return configurator(config);
                }
            }
        }

        internal static void RegisterDefaultProviders()
        {
            RegisterStorageProvider<SQLiteStorageConfig>((c) =>
            {
                return new SQLiteStorageProvider(AccountantPlugin.Instance, c);
            });
        }

        protected Account CreateAccount(long id)
        {
            return new Account(Manager, id);
        }

        protected void ReadMetadata(Account account, Dictionary<string, string> kvs)
        {
            account.DeserializeMetadata(kvs);
        }
    }
}
