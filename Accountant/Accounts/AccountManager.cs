using Accountant.Exceptions;
using Accountant.Storage;

using SharedUtils.OOPUtils;
using SharedUtils.References;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
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

        public bool CreateAccount(string username, string password, out ObjectReference<Account> refn)
        {
            //TODO: Create with placeholder password, then on success hash it and update?
            //Unsure, this might be a lot of labour to perform upfront without guarantee of success.

            Account account = new Account(this, Provider, -1)
            {
                Username = username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password)
            };

            //SaveAccount with an unset identifier (-1) causes an insert/create rather than update.

            var result = Provider.SaveAccount(account);

            if (result.Success)
            {
                if (!TryAdd(account, out refn))
                    return false;
            }
            else
            {
                refn = null;
            }

            return result.Success;
        }

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
