using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityUtilities.Networking;

namespace UnityUtilities.Networking
{
    public abstract class NetBehaviour : MonoBehaviour, CEventListener
    {
        private bool _isOwner = true;
        public bool isOwner
        {
            get
            {
                return _isOwner;
            }
            set
            {
                _isOwner = value;
                state.isOwner = value;
            }
        }

        public int owner = -1;

        public NetworkState state { get; private set; } = new NetworkState(-1, true);

        private NetGUID _guid;
        public NetGUID guid
        {
            get
            {
                return _guid;
            }
            private set
            {
                this._guid = value;
                state = new NetworkState(_guid.id, state, isOwner);
            }
        }

        public int id
        {
            get
            {
                if(guid == null)
                {
                    return -1;
                }
                else
                {
                    return guid.id;
                }
            }
        }

        public int GUID
        {
            get
            {
                return guid.id;
            }
        }

        public static NetBehaviour Create<T>(GameObject gameObject, int owner, NetGUID guid) where T : NetBehaviour
        {
            var behaviour = gameObject.AddComponent<T>();
            behaviour.guid = guid;
            guid.behaviour = behaviour;

            behaviour.PrivateInit(owner);

            behaviour.Init();

            return behaviour;
        }

        private void Awake()
        {
            _Awake();
        }

        protected virtual void _Awake()
        {

        }

        bool init = true;
        private void PrivateInit(int owner)
        {
            init = false;

            this.owner = owner;

            isOwner = owner == NetworkClient.ClientNumber;

            if(owner == NetworkClient.ClientNumber)
            {
                NetBehaviourTracker.AddNetBehaviour(this);
                var spawn = GetSpawn();
                if(spawn != null)
                {
                    CEventSystem.Broadcast(NetBehaviourSpawner.Channel.channel, NetBehaviourSpawner.Channel.subchannel, spawn);
                }
            }
        }

        /// <summary>
        /// Gets called after Awake, after Create, before Start
        /// </summary>
        protected virtual void Init()
        {

        }

        private void Start()
        {
            if(init)
            {
                PrivateInit(NetworkClient.ClientNumber);
            }
            _Start();
        }

        protected virtual void _Start()
        {

        }

        private void OnDestroy()
        {
            NetBehaviourTracker.RemoveNetBehaviour(this);
            if(guid != null)
            {
                GUIDPool.Free(guid.id);
            }
            _OnDestroy();
        }

        protected virtual void _OnDestroy()
        {

        }

        private void Update()
        {
            if(isOwner)
                RunByOwner();
            else
                RunByClient();

            state?.Sync();
        }

        protected virtual void RunByOwner()
        {

        }

        protected virtual void RunByClient()
        {

        }

        public abstract void AcceptEvent(CEvent e);

        public void SetGUID(NetGUID guid)
        {
            this.guid = guid;
        }

        public abstract NetBehaviourSpawn GetSpawn();

        public void BroadcastToSelf(NetworkedCEvent e)
        {
            NetGUID.SendEventToServer(guid.id, e);
        }

        public void BroadcastToSelfUdp(NetworkedCEvent e)
        {
            NetGUID.SendEventToServerUdp(guid.id, e);
        }
    }
}

public static class NetworkBehaviourExtensions
{
    public static void AddNetComponent<T>(this GameObject gameObject) where T : NetBehaviour
    {
        new NetGUID((x) => NetBehaviour.Create<T>(gameObject, NetworkClient.ClientNumber, x));
    }
}