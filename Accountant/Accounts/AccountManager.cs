using Accountant.Accounts.Enums;
using Accountant.Events.Definitions.Accounts;
using Accountant.Exceptions;
using Accountant.Extensions;
using Accountant.Storage;

using SharedUtils.OOPUtils;
using SharedUtils.References;
using SharedUtils.References.Enums;
using SharedUtils.References.Managers.Slim;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Accountant.Accounts
{
    public class AccountManager : AsyncSlimManager<long, Account>
    {
        public StorageProvider Provider { get; private set; }

        public AccountantPlugin Plugin { get; private set; }

        internal AccountManager(AccountantPlugin plugin, StorageProvider provider)
        {
            Plugin = plugin;
            Provider = provider;
        }

        public async Task<(AccountCreateResult result, ObjectReference<Account> refn)> CreateAccount(string username, string password)
        {
            Account account = new Account(this, Provider, -1)
            {
                Username = username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password)
            };

            AccountCreateEvent ace = new AccountCreateEvent(account, AccountantPlugin.Server);

            AccountantPlugin.Server.Events.FireEvent(ace);

            if (ace.Cancelled)
                return (AccountCreateResult.PluginBlocked, null);

            //SaveAccount with an unset identifier (-1) causes an insert/create rather than update.

            var result = await Provider.SaveAccount(account).ConfigureAwait(false);

            if (result.Success)
            {
                if (!TryAdd(account, out var refn))
                    return (AccountCreateResult.AlreadyExists, null);
                else
                    return (AccountCreateResult.Success, refn.ObjectReference);
            }
            else
            {
                if (result.Exception is EntryAlreadyExistsException)
                    return (AccountCreateResult.AlreadyExists, null);
                else
                    return (AccountCreateResult.StorageError, null);
            }
        }

        public async Task<AccountDeleteResult> DeleteAccount(string username)
        {
            //Find the account. If it throws, then it doesn't exist, return false.

            ObjectReference<Account> refn = null;

            try
            {
                refn = await GetAccountByUsername(username).ConfigureAwait(true);
            }
            catch
            {
                return AccountDeleteResult.NotFound;
            }

            AccountDeleteEvent ade = new AccountDeleteEvent(refn.Object, AccountantPlugin.Server);

            AccountantPlugin.Server.Events.FireEvent(ade);

            if (ade.Cancelled)
            {
                refn.Dispose();
                return AccountDeleteResult.PluginBlocked;
            }

            var result = await Provider.DeleteAccount(refn.Object).ConfigureAwait(true);

            if (result.Success)
            {
                AccountantPlugin.Server.Players.ForEach(ply =>
                {
                    if (ply.Signout(refn.Object))
                    {
                        ply.SendInfoMessage("You have been logged out because your account has been deleted.");
                    }
                });

                return AccountDeleteResult.Success;
            }
            else
            {
                return AccountDeleteResult.StorageError;
            }
        }

        /// <summary>
        /// Looks up an account by its username.
        /// <para>This will first check local cache, then query the storage provider.</para>
        /// This always throws an exception from the storage provider if the account doesn't exist!
        /// </summary>
        /// <param name="username"></param>
        /// <exception cref="DataIntegrityException">When multiple accounts match a name due to database error.</exception>
        /// <exception cref="Exception">If the provider fails to fetch an account.</exception>
        /// <returns></returns>
        public async Task<ObjectReference<Account>> GetAccountByUsername(string username)
        {
            var list = Find((acc) => acc.Username == username);

            if (list.Count == 0)
            {

                async ValueTask<Account> factory()
                {
                    var res = await Provider.GetAccount(username).ConfigureAwait(false);

                    return res.Object<Account>() ?? throw res.Exception;
                }

                var acc = await factory().ConfigureAwait(false);

                var res = await AddOrGet(acc.Identifier, factory).ConfigureAwait(false);

                if (res.Success)
                {
                    return res.ObjectReference;
                }
                else if (res.Result == EAddOrGetResult.LoadError)
                {
                    throw res.LoadError;
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported AddOrGet result: {res.Result}");
                }
            }
            else
            {
                if (list.Count > 1)
                {
                    foreach (var x in list)
                        x.Dispose();

                    throw new DataIntegrityException("Multiple accounts matching the same name were found.");
                }
                else
                {
                    return list[0];
                }
            }
        }

        public async Task<ObjectReference<Account>> GetAccountById(long id)
        {
            async ValueTask<Account> factory()
            {
                var res = await Provider.GetAccount(id).ConfigureAwait(false);
                return res.Object<Account>() ?? throw res.Exception;
            }

            var res = await AddOrGet(id, factory).ConfigureAwait(false);

            if (res.Success)
                return res.ObjectReference;
            else if (res.Result == EAddOrGetResult.LoadError)
                throw res.LoadError;
            else
                throw new InvalidOperationException($"Unsupported AddOrGet result: {res.Result}");
        }

        public async Task<PlayerAutoLogins> GetAutoLoginsByUUID(string uuid)
        {
            var res = await Provider.GetAutologinEntries(uuid).ConfigureAwait(false);

            return res.Object<PlayerAutoLogins>() ?? throw res.Exception;
        }

        internal ObjectReference<Account> CreateReference(Account account)
        {
            return References.Create(account);
        }

        public override bool Initialize()
        {
            return true;
        }

        protected override Account CreateObject(Action<Account> initializer = null)
        {
            throw new NotImplementedException();
        }
    }
}
