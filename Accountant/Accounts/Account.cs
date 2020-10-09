using Accountant.Storage;

using SharedUtils.Generic;
using SharedUtils.OOPUtils;
using SharedUtils.References;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Accounts
{
    public class Account : IIdentifiable<long>
    {
        private readonly AccountManager Manager;
        private readonly StorageProvider Provider;

        internal const string AutoLoginMetaKey = "auto_login_uuids";

        internal Account(AccountManager manager, StorageProvider provider, long id)
        {
            Manager = manager;
            Provider = provider;
            Identifier = id;
            Metadata = new Dictionary<string, object>();
        }

        public long Identifier { get; internal set; }

        public string Username;

        public string Password;

        internal Dictionary<string, object> Metadata;

        public bool Save()
        {
            return Provider.SaveAccount(this).Success;
        }

        public bool GetMetadata<T>(string key, out T value)
        {
            value = default;

            if(Metadata.TryGetValue(key, out object val))
            {
                if (val is T)
                {
                    value = (T)val;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public void SetMetadata<T>(string key, T value)
        {
            Metadata[key] = value;
        }

        internal void UpdateLogonTime()
        {
            long utc_time = TimeUtils.UtcUnixMillis;

            SetMetadata("last_logon", utc_time);

            Save();
        }

        public ObjectReference<Account> GetReference()
        {
            return Manager.CreateReference(this);
        }
    }
}
