using Accountant.Accounts.Enums;
using Accountant.Events.Definitions.Accounts;
using Accountant.Exceptions;
using Accountant.Extensions;
using Accountant.Storage;

using SharedUtils.OOPUtils;
using SharedUtils.References;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Xml;

namespace Accountant.Accounts
{
    public class AccountManager : Manager<long, Account>
    {
        public StorageProvider Provider { get; private set; }

        internal AccountManager(StorageProvider provider)
        {
            Provider = provider;
        }

        public AccountCreateResult CreateAccount(string username, string password, out ObjectReference<Account> refn)
        {
            refn = null;

            //TODO: Create with placeholder password, then on success hash it and update?
            //Unsure, this might be a lot of labour to perform upfront without guarantee of success.

            Account account = new Account(this, Provider, -1)
            {
                Username = username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password)
            };

            AccountCreateEvent ace = new AccountCreateEvent(account, AccountantPlugin.Server);

            AccountantPlugin.Server.Events.FireEvent(ace);

            if (ace.Cancelled)
                return AccountCreateResult.PluginBlocked;

            //SaveAccount with an unset identifier (-1) causes an insert/create rather than update.

            var result = Provider.SaveAccount(account);

            if (result.Success)
            {
                if (!TryAdd(account, out refn))
                    return AccountCreateResult.AlreadyExists;
                else
                    return AccountCreateResult.Success;
            }
            else
            {
                if (result.Exception is EntryAlreadyExistsException)
                    return AccountCreateResult.AlreadyExists;
                else
                    return AccountCreateResult.StorageError;
            }
        }

        public AccountDeleteResult DeleteAccount(string username)
        {
            //Find the account. If it throws, then it doesn't exist, return false.

            ObjectReference<Account> refn = null;

            try
            {
                refn = GetAccountByUsername(username);
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

            var adresult = AccountDeleteResult.NotFound;

            Synchroniser.Synchronise(refn.Object, (_) =>
            {
                var result = Provider.DeleteAccount(refn.Object);

                if(result.Success)
                {
                    //Deauthenticate everyone logged into this account.
                    //The synchroniser lock ensures no new logins to this account can occur.

                    AccountantPlugin.Server.Players.ForEach(ply =>
                    {
                        if (ply.Signout(refn.Object))
                        {
                            ply.SendInfoMessage("You have been logged out because your account has been deleted.");
                        }
                    });

                    adresult = AccountDeleteResult.Success;
                }
                else
                {
                    adresult = AccountDeleteResult.StorageError;
                }

            });

            refn.Dispose();

            return adresult;
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
        public ObjectReference<Account> GetAccountByUsername(string username)
        {
            var list = Find((acc) => acc.Username == username);

            if (list.Count == 0)
            {

                Account factory()
                {
                    var res = Provider.GetAccount(username);

                    return res.Object<Account>() ?? throw res.Exception;
                }

                var acc = factory();

                return AddOrGet(acc.Identifier, factory);
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

        public ObjectReference<Account> GetAccountById(long id)
        {
            Account factory()
            {
                var res = Provider.GetAccount(id);
                return res.Object<Account>() ?? throw res.Exception;
            }

            return AddOrGet(id, factory);
        }

        public PlayerAutoLogins GetAutoLoginsByUUID(string uuid)
        {
            var res = Provider.GetAutologinEntries(uuid);

            return res.Object<PlayerAutoLogins>() ?? throw res.Exception;
        }

        internal ObjectReference<Account> CreateReference(Account account)
        {
            return References.Create(account);
        }
    }
}
