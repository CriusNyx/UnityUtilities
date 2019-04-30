using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    Queue<Tuple<Enum, Enum, CEvent>> debug_EventQueue = new Queue<Tuple<Enum, Enum, CEvent>>();
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
        if(instance != null)
            instance._AddEventListener(channel, subchannel, listener);
    }

    private void _AddEventListener(Enum channel, Enum subchannel, CEventListener listener)
    {
        EnsureList(channel, subchannel);
        eventListeners[channel][subchannel].Add(listener);
    }

    public static void RemoveEventListener(Enum channel, Enum subchannel, CEventListener listener)
    {
        if(instance != null)
            instance._RemoveEventListener(channel, subchannel, listener);
    }

    private void _RemoveEventListener(Enum channel, Enum subchannel, CEventListener listener)
    {
        EnsureList(channel, subchannel);
        eventListeners[channel][subchannel].Remove(listener);
    }

    private void EnsureList(Enum channel, Enum subchannel)
    {
        if(!eventListeners.ContainsKey(channel))
        {
            eventListeners[channel] = new Dictionary<Enum, List<CEventListener>>();
        }
        if(!eventListeners[channel].ContainsKey(subchannel))
        {
            eventListeners[channel][subchannel] = new List<CEventListener>();
        }
    }

    public static void Broadcast(Enum channel, Enum subchannel, CEvent e)
    {
        if(instance != null)
            instance._Broadcast(channel, subchannel, e);
    }

    private void _Broadcast(Enum channel, Enum subchannel, CEvent e)
    {
        EnsureList(channel, subchannel);
        foreach(var listener in eventListeners[channel][subchannel])
        {
            listener.AcceptEvent(e);
        }
#if UNITY_EDITOR
        debug_EventQueue.Enqueue(new Tuple<Enum, Enum, CEvent>(channel, subchannel, e));
        while(debug_EventQueue.Count > maxQueueLength)
            debug_EventQueue.Dequeue();
#endif
    }

#if UNITY_EDITOR
    public static Dictionary<Enum, Dictionary<Enum, List<CEventListener>>> GetEventListeners()
    {
        if(instance != null)
        {
            return instance._GetEventListeners();
        }
        return null;
    }

    private Dictionary<Enum, Dictionary<Enum, List<CEventListener>>> _GetEventListeners()
    {
        return eventListeners;
    }

    public static IEnumerable<Tuple<Enum, Enum, CEvent>> GetBroadcastList()
    {
        if(instance != null)
        {
            return instance._GetBroadcastList();
        }
        return null;
    }

    private IEnumerable<Tuple<Enum, Enum, CEvent>> _GetBroadcastList()
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

public abstract class CEvent
{

}

public interface CEventListener
{
    void AcceptEvent(CEvent e);
}