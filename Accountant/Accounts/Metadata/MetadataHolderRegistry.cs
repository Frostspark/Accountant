using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Accountant.Accounts.Metadata.Builtin;
using Dahomey.Json.Serialization.Conventions;
using SharedUtils.Generic;

namespace Accountant.Accounts.Metadata
{
    public sealed class MetadataHolderRegistry
    {
        public delegate MetadataHolder<T> MetadataHolderCreator<T>(T value);
        public delegate MetadataHolder MetadataHolderCreatorIntern(object value);

        internal MetadataHolderRegistry()
        {

        }

        private readonly object SyncRoot = new();

        private ReadOnlyDictionary<Type, MetadataHolderCreatorIntern> Creators = new ReadOnlyDictionary<Type, MetadataHolderCreatorIntern>(new Dictionary<Type, MetadataHolderCreatorIntern>());

        internal MetadataHolder<T> CreateHolder<T>(T obj)
        {
            var cr = Creators;

            if (cr.TryGetValue(typeof(T), out var creator))
            {
                var mh = creator(obj);

                if (mh is MetadataHolder<T> mht)
                    return mht;
                else
                    throw new InvalidOperationException($"Instantiator for holder <{typeof(T).Name}> returns non-holder type.");
            }
            else
            {
                throw new InvalidOperationException("Unknown holder type!");
            }
        }

        internal void SetupDefaults()
        {
            RegisterHolder<int>((i) => new IntMetadataHolder() { Value = i });
            RegisterHolder<uint>((ui) => new UIntMetadataHolder() { Value = ui });
            RegisterHolder<short>((sh) => new ShortMetadataHolder() { Value = sh });
            RegisterHolder<ushort>((ush) => new UShortMetadataHolder() { Value = ush });
            RegisterHolder<long>((l) => new LongMetadataHolder() { Value = l });
            RegisterHolder<ulong>((ul) => new ULongMetadataHolder() { Value = ul });
            RegisterHolder<byte>((b) => new ByteMetadataHolder() { Value = b });
            RegisterHolder<sbyte>((sb) => new SByteMetadataHolder() { Value = sb });
            RegisterHolder<string>((str) => new StringMetadataHolder() { Value = str });
        }

        public bool RegisterHolder<T>(MetadataHolderCreator<T> creator)
        {
            var type = typeof(T);
            MetadataHolderCreator<T> mhc = creator;

            MetadataHolder intern(object obj) => mhc((T)obj);

            lock (SyncRoot)
            {
                var cr = Creators.ToDictionary();

                if (cr.TryAdd(type, intern))
                {
                    Creators = cr.ToReadOnly();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeregisterHolder<T>()
        {
            var type = typeof(T);

            lock (SyncRoot)
            {
                var cr = Creators;

                if (cr.ContainsKey(type))
                {
                    var cr2 = cr.ToDictionary();

                    cr2.Remove(type);

                    Creators = cr2.ToReadOnly();

                    return true;
                }

                return false;
            }
        }

        internal void PushRegisteredTypes(DiscriminatorConventionRegistry registry)
        {
            var cr = Creators;

            foreach(var x in cr)
            {
                registry.RegisterType(x.Key);
            }
        }
    }
}
