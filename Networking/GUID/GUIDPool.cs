using System.Collections.Generic;
using UnityEngine;

namespace UnityUtilities.Networking
{
    [AutoInit]
    public class GUIDPool : MonoBehaviour
    {
        private Queue<NetGUID> waitingGUIDs = new Queue<NetGUID>();

        private static GUIDPool instance;

        public Dictionary<int, NetGUID> map = new Dictionary<int, NetGUID>();

        static int current = 0;

        public void Awake()
        {
            instance = this;
            foreach(ReservedGUID res in FindObjectsOfType<ReservedGUID>())
            {
                var id = new NetGUID(res.id);
                NetBehaviour netBehaviour = res.GetComponent<NetBehaviour>();
                netBehaviour?.SetGUID(id);
                id.behaviour = netBehaviour;

                current = System.Math.Max(current, res.id + 1);
            }
        }

        public static void AddGUID(NetGUID guid)
        {
            instance?._AddGUID(guid);
        }

        private void _AddGUID(NetGUID guid)
        {
            waitingGUIDs.Enqueue(guid);
            NetworkClient.SendObjectToServerTCP(NetworkControlCode.runOnServer, new GUIDRequest());
        }

        public static void AcceptGUID(int guid)
        {
            instance?._AcceptGUID(guid);
        }

        private void _AcceptGUID(int guid)
        {
            var next = waitingGUIDs.Dequeue();
            next.SetGUID(guid);
        }

        public static void Free(int guid)
        {
            instance?._Free(guid);
        }

        private void _Free(int guid)
        {
            if(map.ContainsKey(guid))
            {
                map.Remove(guid);
            }
        }


        public static int GetGUID()
        {
            int output = current;
            current++;
            return output;
        }

        public static void AddToMap(NetGUID guid)
        {
            instance?._AddToMap(guid);
        }

        private void _AddToMap(NetGUID guid)
        {
            map.Add(guid.id, guid);
        }

        public static NetGUID GetGUID(int id)
        {
            return instance?._GetGUID(id);
        }

        private NetGUID _GetGUID(int id)
        {
            try
            {
                return map[id];
            }
            catch
            {
                return null;
            }
        }

        public static NetBehaviour GetBehaviour(int id)
        {
            return instance?._GetBehaviour(id);
        }

        private NetBehaviour _GetBehaviour(int id)
        {
            try
            {
                return map[id].behaviour;
            }
            catch
            {
                return null;
            }
        }

        public static T GetObject<T>(int id) where T : Component
        {
            NetBehaviour behaviour = GetBehaviour(id);
            return behaviour?.GetComponent<T>();
        }

#if UNITY_EDITOR
        public static IEnumerable<KeyValuePair<int, NetGUID>> GetMappedIDs()
        {
            return instance?._GetMappedIDs();
        }

        private IEnumerable<KeyValuePair<int, NetGUID>> _GetMappedIDs()
        {
            return map;
        }
#endif
    }
}