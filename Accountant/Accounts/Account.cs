using Accountant.Accounts.Metadata;
using Accountant.Storage;

using SharedUtils.Generic;
using SharedUtils.OOPUtils;
using SharedUtils.References;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace Accountant.Accounts
{
    /// <summary>
    /// Represents a user account.
    /// <para>This class is not guaranteed to be thread safe.</para>
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
        }

        private readonly object SyncRoot = new();

        public long Identifier { get; internal set; }

        public string Username;

        public string Password;

        private ReadOnlyDictionary<string, object> Metadata = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

        public bool Save()
        {
            lock (SyncRoot)
                return Provider.SaveAccount(this).Success;
        }

        public bool TryGetMetadata<T>(string key, out T value)
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
            lock (SyncRoot)
            {
                var m = Metadata.ToDictionary();

                m[key] = Manager.Plugin.MetadataRegistry.CreateHolder(value);

                Metadata = m.ToReadOnly();

                Save();
            }
        }

        internal void SetMetadataWrapped(string key, object obj)
        {
            lock(SyncRoot)
            {
                var m = Metadata.ToDictionary();

                m[key] = obj;

                Metadata = m.ToReadOnly();
            }
        }

        public bool TryRemoveMetadata<T>(string t, out T value)
        {
            value = default;

            lock (SyncRoot)
            {
                var m = Metadata;

                if(m.ContainsKey(t))
                {
                    var m2 = Metadata.ToDictionary();

                    m2.Remove(t, out var val);

                    if(val is MetadataHolder<T> mht)
                    {
                        value = mht.Value;
                    }
                    else if(val is MetadataHolder)
                    {
                        Manager.Plugin.Log.LogWarning($"Call to TryRemoveMetadata specified incorrect generic type to provide out value to, returning default! (holder type is {val.GetType().Name}, caller provided {typeof(MetadataHolder<T>).Name})");
                    }
                    else
                    {
                        Manager.Plugin.Log.LogWarning($"Metadata under key {t} for account {this.Identifier} was not packed inside a MetadataHolder, returning default.");
                    }

                    Metadata = m2.ToReadOnly();

                    Save();

                    return true;
                }

                return false;
            }
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

            var m = Metadata;

            foreach(var x in m)
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
                SetMetadataWrapped(x.Key, x.Value);
            }
        }
    }
}
