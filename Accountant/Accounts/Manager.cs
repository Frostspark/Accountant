using SharedUtils.Synchronisation;
using SharedUtils.Synchronisation.Interfaces;
using SharedUtils.Synchronisation.References;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Accountant.Accounts
{
    public abstract class Manager<Id, Obj> where Obj : class, IIdentifiable<Id>
    {
        protected ConcurrentDictionary<Id, Obj> Objects;

        protected ReferenceManager<Obj> References;

        protected ObjectSynchroniser<Obj> Synchroniser;

        protected Manager()
        {
            Objects = new ConcurrentDictionary<Id, Obj>();
            Synchroniser = new ObjectSynchroniser<Obj>();
            References = new ReferenceManager<Obj>(Synchroniser)
            {
                OnFirstReference = FirstReference,
                OnUnReference = CleanupObject
            };
        }

        protected bool TryAdd(Obj obj, out ObjectReference<Obj> reference)
        {
            reference = null;

            if (!Objects.TryAdd(obj.Identifier, obj))
                return false;

            ObjectReference<Obj> objref = null;

            if(!Synchroniser.CreateSynchroniser(obj, () => objref = References.Create(obj)))
            {
                Objects.TryRemove(obj.Identifier, out _);
                return false;
            }

            reference = objref;

            return true;
        }

        protected bool TryRemove(Obj obj, bool force = false)
        {
            if (!Objects.TryGetValue(obj.Identifier, out var obj2) || obj != obj2)
                return true;

            bool freed = false;

            Synchroniser.Synchronise(obj, (x) =>
            {
                if (!References.FreeObject(obj, force))
                    return;

                freed = true;
                Synchroniser.RemoveSynchroniser(obj);
            });

            if (!freed)
                return false;

            Objects.TryRemove(obj.Identifier, out _);

            return true;
        }

        /// <summary>
        /// Filters all objects using the <paramref name="filter"/> and returns a list of matching results' references.
        /// <para>All references you receive from this method must be disposed by you.</para>
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected List<ObjectReference<Obj>> Find(Predicate<Obj> filter)
        {
            List<ObjectReference<Obj>> list = new List<ObjectReference<Obj>>();

            foreach(var pair in Objects)
            {
                var reference = References.Create(pair.Value);

                if (reference == null)
                    continue;

                if (filter(reference.Object))
                {
                    list.Add(reference);
                }
                else
                {
                    reference.Dispose();
                }
            }

            return list;
        }

        /// <summary>
        /// Adds or retrieves an object from storage.
        /// <para>The object factory is expected to always return a valid object, or throw.</para>
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        protected ObjectReference<Obj> AddOrGet(Id identifier, Func<Obj> factory)
        {
            ObjectReference<Obj> reference = null;

            while (true)
            {
                if (Objects.TryGetValue(identifier, out var obj))
                {
                    if (Synchroniser.Synchronise(obj, (_) => { reference = References.Create(obj); }))
                    {
                        //Synchronisation succeeded, we can return this reference.
                        return reference;
                    }
                }
                else
                {
                    if (TryAdd(factory(), out reference))
                    {
                        //Added successfully, return the reference.
                        return reference;
                    }
                }
            }
        }

        private void FirstReference(Obj obj)
        {
            AccountantPlugin.Instance.Log.LogDebug($"Account {obj.Identifier} pinned.");
        }

        private void CleanupObject(Obj obj)
        {
            AccountantPlugin.Instance.Log.LogDebug($"Account {obj.Identifier} unloading...");
            TryRemove(obj);
        }

    }
}
