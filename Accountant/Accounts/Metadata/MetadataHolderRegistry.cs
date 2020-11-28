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
        public delegate MHT MetadataHolderCreator<T, MHT>(T value);
        public delegate MetadataHolder MetadataHolderCreatorIntern(object value);

        internal MetadataHolderRegistry()
        {

        }

        private readonly object SyncRoot = new();

        private ReadOnlyDictionary<Type, MetadataHolderCreatorIntern> Creators = new ReadOnlyDictionary<Type, MetadataHolderCreatorIntern>(new Dictionary<Type, MetadataHolderCreatorIntern>());
        private ReadOnlyCollection<Type> MetadataHolderTypes = new ReadOnlyCollection<Type>(new List<Type>());

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
                throw new InvalidOperationException($"No holder type is defined for type {typeof(T).Name}");
            }
        }

        internal void SetupDefaults()
        {
            RegisterHolder<int, IntMetadataHolder>((i) => new IntMetadataHolder() { Value = i });
            RegisterHolder<uint, UIntMetadataHolder>((ui) => new UIntMetadataHolder() { Value = ui });
            RegisterHolder<short, ShortMetadataHolder>((sh) => new ShortMetadataHolder() { Value = sh });
            RegisterHolder<ushort, UShortMetadataHolder>((ush) => new UShortMetadataHolder() { Value = ush });
            RegisterHolder<long, LongMetadataHolder>((l) => new LongMetadataHolder() { Value = l });
            RegisterHolder<ulong, ULongMetadataHolder>((ul) => new ULongMetadataHolder() { Value = ul });
            RegisterHolder<byte, ByteMetadataHolder>((b) => new ByteMetadataHolder() { Value = b });
            RegisterHolder<sbyte, SByteMetadataHolder>((sb) => new SByteMetadataHolder() { Value = sb });
            RegisterHolder<string, StringMetadataHolder>((str) => new StringMetadataHolder() { Value = str });
        }

        public bool RegisterHolder<T, MHT>(MetadataHolderCreator<T, MHT> creator) where MHT : MetadataHolder<T>
        {
            var type = typeof(T);
            var concreteholdertype = typeof(MHT);

            MetadataHolderCreator<T, MHT> mhc = creator;

            MetadataHolder intern(object obj) => mhc((T)obj);

            lock (SyncRoot)
            {
                var cr = Creators.ToDictionary();

                var hts = MetadataHolderTypes.ToList();

                if (cr.TryAdd(type, intern) && !hts.Contains(concreteholdertype))
                {
                    Creators = cr.ToReadOnly();

                    hts.Add(concreteholdertype);

                    MetadataHolderTypes = hts.ToReadOnly();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeregisterHolder<T, MHT>() where MHT : MetadataHolder<T>
        {
            var type = typeof(T);
            var ctype = typeof(MHT);

            lock (SyncRoot)
            {
                var cr = Creators;
                var mhts = MetadataHolderTypes;

                if (cr.ContainsKey(type) && mhts.Contains(ctype))
                {
                    var cr2 = cr.ToDictionary();

                    cr2.Remove(type);

                    Creators = cr2.ToReadOnly();

                    var mhts2 = mhts.ToList();

                    mhts2.Remove(ctype);

                    MetadataHolderTypes = mhts2.ToReadOnly();

                    return true;
                }

                return false;
            }
        }

        internal void PushRegisteredTypes(DiscriminatorConventionRegistry registry)
        {
            var types = MetadataHolderTypes;

            foreach(var type in types)
            {
                registry.RegisterType(type);
            }
        }
    }
}
