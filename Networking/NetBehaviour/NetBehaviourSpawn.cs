using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtilities;

[System.Serializable]
public abstract class NetBehaviourSpawn : NetworkedCEvent
{
    public readonly int guid;

    protected NetBehaviourSpawn(int guid)
    {
        this.guid = guid;
    }

    public abstract void Spawn();
}