using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtilities;
using UnityUtilities.Networking;

namespace UnityUtilities.Networking
{
    public class ServerDebugListener : MonoBehaviour, CEventListener
    {
        void Start()
        {
            CEventSystem.AddEventListener(ServerEventChannel.channel, ServerEventChannel.subchannel, this);
        }

        void OnDestroy()
        {
            CEventSystem.RemoveEventListener(ServerEventChannel.channel, ServerEventChannel.subchannel, this);
        }

        public void AcceptEvent(CEvent e)
        {
            if (e is ServerDebugEvent se)
            {
                Debug.Log(se.text);
            }
            if(e is ClientDebugEvent ce)
            {
                Debug.Log(ce.text);
            }
        }

        [Serializable]
        public class ServerDebugEvent : NetworkedCEvent
        {
            public readonly string text;

            public override NetworkControlCode eventType { get; } = NetworkControlCode.runOnServer;

            public ServerDebugEvent(string text)
            {
                this.text = text;
            }
        }

        [Serializable]
        public class ClientDebugEvent : NetworkedCEvent
        {
            public readonly string text;

            public override NetworkControlCode eventType { get; } = NetworkControlCode.runOnClient;

            public ClientDebugEvent(string text)
            {
                this.text = text;
            }
        }
    }
}