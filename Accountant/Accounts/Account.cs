using Accountant.Accounts.Metadata;
using Accountant.Storage;

using SharedUtils.Generic;
using SharedUtils.OOPUtils;
using SharedUtils.References;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Accountant.Accounts
{
    /// <summary>
    /// Represents a user account.
    /// <para>This class is not thread safe.</para>
    /// </summary>
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

        private Dictionary<string, object> Metadata;

        public bool Save()
        {
            return Provider.SaveAccount(this).Success;
        }

        public bool GetMetadata<T>(string key, out T value)
        {
            value = default;

            if (Metadata.TryGetValue(key, out object val))
            {
                if (val is MetadataHolder<T> mht)
                {
                    value = mht.Value;
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
            Metadata[key] = Manager.Plugin.MetadataRegistry.CreateHolder(value);
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

        internal Dictionary<string, string> SerializeMetadata()
        {
            JsonSerializerOptions jso = MetadataHelpers.SetupMetadataSerializer(Manager.Plugin);

            Dictionary<string, string> kvs = new Dictionary<string, string>();

            foreach(var x in Metadata)
            {
                kvs.Add(x.Key, JsonSerializer.Serialize(x.Value, jso));
            }

            return kvs;
        }

        internal void DeserializeMetadata(Dictionary<string, string> jsonkvs)
        {
            JsonSerializerOptions jso = MetadataHelpers.SetupMetadataSerializer(Manager.Plugin);

            Dictionary<string, object> deserialized = new Dictionary<string, object>();

            foreach (var x in jsonkvs)
            {
                try
                {
                    var obj = JsonSerializer.Deserialize<MetadataHolder>(x.Value, jso);

                    deserialized[x.Key] = obj;
                }
                catch (Exception e)
                {
                    throw new Exception($"Invalid JSON for user account {Identifier} (uname {Username}) at key {x.Key}.", e);
                }
            }

            //No exceptions thrown means this is now safe to merge into the state.

            foreach(var x in deserialized)
            {
                Metadata[x.Key] = x.Value;
            }
        }
    }
}
