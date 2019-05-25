using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities.Networking
{
    public class NetworkState : IEnumerable<KeyValuePair<Enum, object>>, IEnumerable<(Enum, object)>
    {
        public readonly int guid;
        public bool isOwner;

        private Dictionary<Enum, object> objects = new Dictionary<Enum, object>();
        private Dictionary<Enum, object> diffs = new Dictionary<Enum, object>();

        private object stateLock = new object();

        #region Construction
        private static Dictionary<int, NetworkState> networkStateMap = new Dictionary<int, NetworkState>();

        public NetworkState(int guid, bool isOwner = true)
        {
            this.guid = guid;
            this.isOwner = isOwner;
            if(guid >= 0)
            {
                networkStateMap.Add(guid, this);
            }
        }

        public NetworkState(int guid, IEnumerable<(Enum, object)> objects, bool isOwner = true) : this(guid, isOwner)
        {
            if(objects != null)
            {
                lock(stateLock)
                {
                    foreach(var (key, value) in objects)
                    {
                        this.objects[key] = value;
                    }
                }
            }
        }

        //~NetworkState()
        //{
        //    if(networkStateMap[guid] == this)
        //    {
        //        networkStateMap.Remove(guid);
        //    }
        //    Debug.Log("Removed Network State: " + guid);
        //}
        #endregion

        public object this[Enum ident]
        {
            get
            {
                lock(stateLock)
                {
                    return objects[ident];
                }
            }
            set
            {
                if(!isOwner)
                {
                    throw new NetworkOwnerException("Cannot assign state, this object is now the owner");
                }
                lock(stateLock)
                {
                    objects[ident] = value;
                    diffs[ident] = value;
                }

            }
        }

        public class NetworkOwnerException : Exception
        {
            public NetworkOwnerException()
            {

            }
            public NetworkOwnerException(string message) : base(message)
            {

            }
        }

        private IEnumerable<(Enum, object)> GetDiffs()
        {
            lock(stateLock)
            {
                foreach(var diff in diffs)
                {
                    yield return (diff.Key, diff.Value);
                }
                diffs = new Dictionary<Enum, object>();
            }
        }

        public IEnumerator<KeyValuePair<Enum, object>> GetEnumerator()
        {
            lock(stateLock)
            {
                foreach(var o in objects)
                {
                    yield return o;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock(stateLock)
            {
                foreach(var o in objects)
                {
                    yield return o;
                }
            }
        }

        IEnumerator<(Enum, object)> IEnumerable<(Enum, object)>.GetEnumerator()
        {
            lock(stateLock)
            {
                foreach(var o in objects)
                {
                    yield return (o.Key, o.Value);
                }
            }
        }

        public void HardSync()
        {
            if(isOwner)
            {
                foreach(var pair in this)
                {
                    NetworkClient.SendObjectToServerTCP(NetworkControlCode.runOnClient, new NetworkStateSync(guid, pair.Key, pair.Value));
                }
            }
        }

        public void Sync()
        {
            if(isOwner)
            {
                foreach(var (key, value) in GetDiffs())
                {
                    NetworkClient.SendObjectToServerTCP(NetworkControlCode.runOnClient, new NetworkStateSync(guid, key, value));
                }
            }
        }

        public static void ApplySync(NetworkStateSync sync)
        {
            int guid = sync.guid;
            Enum key = sync.key;
            object value = sync.value;

            NetworkState state = networkStateMap[guid];
            if(!state.isOwner)
            {
                lock(state.stateLock)
                {
                    state.objects[key] = value;
                }
            }
        }

        [Serializable]
        public class NetworkStateSync : NetworkedCEvent
        {
            public int guid;
            public Enum key;
            public object value;

            public NetworkStateSync(int guid, Enum key, object value)
            {
                this.guid = guid;
                this.key = key;
                this.value = value;
            }
        }
    }
}