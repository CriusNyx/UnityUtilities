using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityUtilities;
using UnityUtilities.Networking;

class SpawnTestBehaviour : NetBehaviour
{
    public override void AcceptEvent(CEvent e)
    {
        if(e is DestroySelf)
        {
            Destroy(gameObject);
        }
    }

    public override NetBehaviourSpawn GetSpawn()
    {
        return new SpawnTestBehaviourSpawn(guid.id);
    }

    public void Update()
    {
        if(isOwner)
        {
            if(Input.GetKeyDown(KeyCode.W))
            {
                BroadcastToSelf(new DestroySelf());
            }
        }
    }

    [Serializable]
    public class SpawnTestBehaviourSpawn : NetBehaviourSpawn
    {
        public SpawnTestBehaviourSpawn(int guid) : base(guid)
        {

        }

        public override NetworkControlCode eventType => base.eventType;

        public override void Spawn()
        {
            Create<SpawnTestBehaviour>(GameObject.CreatePrimitive(PrimitiveType.Sphere), networkClientNumber, new NetGUID(guid));
        }

        public override string ToString()
        {
            return base.ToString() + " clientNumber: " + networkClientNumber;
        }
    }

    [Serializable]
    public class DestroySelf : NetworkedCEvent
    {

    }
}