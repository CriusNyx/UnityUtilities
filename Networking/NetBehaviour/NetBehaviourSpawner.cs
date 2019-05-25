using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtilities;
using UnityUtilities.Networking;

[AutoInit]
public class NetBehaviourSpawner : MonoBehaviour, CEventListener
{
    public enum Channel
    {
        channel, subchannel
    }

    public void Start()
    {
        CEventSystem.AddEventListener(Channel.channel, Channel.subchannel, this);
    }

    public void AcceptEvent(CEvent e)
    {
        if(e is NetBehaviourSpawn spawn)
        {
            if(spawn.networkClientNumber != NetworkClient.ClientNumber)
            {
                spawn.Spawn();
            }
        }
    }
}