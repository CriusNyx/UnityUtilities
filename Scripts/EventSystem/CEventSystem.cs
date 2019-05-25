using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtilities.ExecutionOrder.ExecutionOrderControl;
using UnityUtilities.Networking;

namespace UnityUtilities
{
    /// <summary>
    /// Event system for broadcasting events across the engine
    /// This event system is designed as a simplified version of the event system used in Snake Attack
    /// Compared to snake attack
    ///     This event system is less prone to message races during initialization, (since it is initialized by another class instead of dynamically)
    ///     This event system is also less prone to initialization problems when loading scenes.
    ///     This also removes the EventSubchannel enum, preventing some common mistakes where EvenChannel was passed instead of EventSubChannel
    /// </summary>
    [AutoInit]
    public class CEventSystem : MonoBehaviour
    {
        Dictionary<Enum, Dictionary<Enum, List<CEventListener>>> eventListeners = new Dictionary<Enum, Dictionary<Enum, List<CEventListener>>>();

#if UNITY_EDITOR
        Queue<(Enum, Enum, CEvent)> debug_EventQueue = new Queue<(Enum, Enum, CEvent)>();
        public static int maxQueueLength = 500;
#endif

        private static CEventSystem instance;

        private void Awake()
        {
            instance = this;
        }
        private void OnDestroy()
        {
            instance = null;
        }

        public static void AddEventListener(Enum channel, Enum subchannel, CEventListener listener)
        {
            if (instance != null)
                instance._AddEventListener(channel, subchannel, listener);
        }

        private void _AddEventListener(Enum channel, Enum subchannel, CEventListener listener)
        {
            EnsureList(channel, subchannel);
            eventListeners[channel][subchannel].Add(listener);
        }

        public static void RemoveEventListener(Enum channel, Enum subchannel, CEventListener listener)
        {
            if (instance != null)
                instance._RemoveEventListener(channel, subchannel, listener);
        }

        private void _RemoveEventListener(Enum channel, Enum subchannel, CEventListener listener)
        {
            EnsureList(channel, subchannel);
            eventListeners[channel][subchannel].Remove(listener);
        }

        private void EnsureList(Enum channel, Enum subchannel)
        {
            if (!eventListeners.ContainsKey(channel))
            {
                eventListeners[channel] = new Dictionary<Enum, List<CEventListener>>();
            }
            if (!eventListeners[channel].ContainsKey(subchannel))
            {
                eventListeners[channel][subchannel] = new List<CEventListener>();
            }
        }

        public static void Broadcast(Enum channel, Enum subchannel, CEvent e)
        {
            if (instance != null)
            {
                instance._Broadcast(channel, subchannel, e);
            }
        }

        private void _Broadcast(Enum channel, Enum subchannel, CEvent e)
        {
            NetworkedCEvent ne = GetNetEvent(e);
            if (ne != null)
            {
                NetworkClient.SendObjectToServerTCP(ne.eventType, new NetworkEventBroadcast(channel, subchannel, ne));
            }
            else
            {
                EnsureList(channel, subchannel);
                foreach (var listener in eventListeners[channel][subchannel])
                {
                    listener.AcceptEvent(e);
                }
            }
#if UNITY_EDITOR
            debug_EventQueue.Enqueue((channel, subchannel, e));
            while (debug_EventQueue.Count > maxQueueLength)
                debug_EventQueue.Dequeue();
#endif
        }

        private NetworkedCEvent GetNetEvent(CEvent e)
        {
            if (e is NetworkedCEvent ne)
            {
                if (ne.sendToServer)
                {
                    return ne;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        public static Dictionary<Enum, Dictionary<Enum, List<CEventListener>>> GetEventListeners()
        {
            if (instance != null)
            {
                return instance._GetEventListeners();
            }
            return null;
        }

        private Dictionary<Enum, Dictionary<Enum, List<CEventListener>>> _GetEventListeners()
        {
            return eventListeners;
        }

        public static IEnumerable<(Enum, Enum, CEvent)> GetBroadcastList()
        {
            if (instance != null)
            {
                return instance._GetBroadcastList();
            }
            return null;
        }

        private IEnumerable<(Enum, Enum, CEvent)> _GetBroadcastList()
        {
            return debug_EventQueue;
        }
#endif
    }

    public enum EventChannel
    {
        none,
        input,
        randomEvents
    }

    public enum EventPlayerNumberChannel
    {
        player1,
        player2,
        player3,
        player4,
    }

    [Serializable]
    public abstract class CEvent
    {

    }

    [Serializable]
    public abstract class NetworkedCEvent : CEvent
    {
        public bool sendToServer = true;
        public readonly int networkClientNumber;

        public virtual NetworkControlCode eventType { get; } = NetworkControlCode.runOnClient;

        public NetworkedCEvent()
        {
            this.networkClientNumber = NetworkClient.ClientNumber;
        }

        public override string ToString()
        {
            return "sendToServer: " + (sendToServer ? "True " : "False ") + base.ToString();
        }
    }

    [Serializable]
    public class NetworkEventBroadcast
    {
        public readonly Enum channel, subchannel;
        public readonly NetworkedCEvent e;

        public NetworkEventBroadcast(Enum channel, Enum subchannel, NetworkedCEvent e)
        {
            this.channel = channel;
            this.subchannel = subchannel;
            this.e = e;
        }

        public override string ToString()
        {
            return "NetworkEventBroadcast(" + channel + ", " + subchannel + "," + e.ToString() + ")";
        }
    }

    public interface CEventListener
    {
        void AcceptEvent(CEvent e);
    }
}