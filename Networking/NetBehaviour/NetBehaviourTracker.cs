using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtilities;
using UnityUtilities.Networking;

[AutoInit]
public class NetBehaviourTracker : MonoBehaviour
{
    private static NetBehaviourTracker instance;

    private HashSet<NetBehaviour> currentNetBehaviours = new HashSet<NetBehaviour>();

    public void Awake()
    {
        instance = this;
    }

    public static void AddNetBehaviour(NetBehaviour netBehaviour)
    {
        instance?._AddNetBehaviour(netBehaviour);
    }

    private void _AddNetBehaviour(NetBehaviour netBehaviour)
    {
        lock(currentNetBehaviours)
        {
            currentNetBehaviours.Add(netBehaviour);
        }
    }

    public static void RemoveNetBehaviour(NetBehaviour netBehaviour)
    {
        instance?._RemoveNetBehaviour(netBehaviour);
    }

    private void _RemoveNetBehaviour(NetBehaviour netBehaviour)
    {
        lock(currentNetBehaviours)
        {
            currentNetBehaviours.Remove(netBehaviour);
        }
    }

    public static IEnumerable<NetBehaviour> GetNetBehaviours()
    {
        if(instance == null)
        {
            return new NetBehaviour[] { };
        }
        else
        {
            return instance._GetNetBehaviours();
        }
    }

    private IEnumerable<NetBehaviour> _GetNetBehaviours()
    {
        lock(currentNetBehaviours)
        {
            foreach(var output in currentNetBehaviours)
            {
                yield return output;
            }
        }
    }
}