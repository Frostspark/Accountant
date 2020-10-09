using Accountant.Accounts;
using Accountant.Configuration.Storage;

using SharedUtils.Storage;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Storage
{
    public abstract class StorageProvider
    {
        protected readonly AccountantPlugin Plugin;

        internal StorageProvider(AccountantPlugin plugin, StorageConfig config)
        {
            Plugin = plugin;
        }

        /// <summary>
        /// Initializes the provider.
        /// <para>Test for success using <see cref="StorageResult.Success"/></para>
        /// </summary>
        public abstract StorageResult Initialize();

        /// <summary>
        /// Deinitialises the provider.
        /// </summary>
        /// <returns></returns>
        public abstract StorageResult Deinitialise();

        /// <summary>
        /// Deletes an account.
        /// <para>Test for success using <see cref="StorageResult.Success"/></para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract StorageResult DeleteAccount(Account acc);

        /// <summary>
        /// Retrieves an account by id, or throws an exception.
        /// <para>Retrieve object with <see cref="StorageResult.Object{Account}"/> where T = <see cref="Account"/></para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract StorageResult GetAccount(long id);

        /// <summary>
        /// Retrieves an account by username, or throws an exception.
        /// <para>Retrieve object with <see cref="StorageResult.Object{Account}"/> where T = <see cref="Account"/></para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract StorageResult GetAccount(string name);

        /// <summary>
        /// Saves an account.
        /// <para>Test for success using <see cref="StorageResult.Success"/></para>
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public abstract StorageResult SaveAccount(Account account);

        /// <summary>
        /// Retrieves auto-login entries for a given uuid.
        /// <para>Retrieve object with <see cref="StorageResult.Object{AutoLoginInfo}"/> where T = <see cref="PlayerAutoLogins"/></para>
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public abstract StorageResult GetAutologinEntries(string uuid);
    }
}
