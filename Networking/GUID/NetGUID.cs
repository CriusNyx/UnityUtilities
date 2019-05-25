using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    [Serializable]
    public class NetGUID
    {
        public int id { get; private set; }
        private readonly Action<NetGUID> eventResponse;

        public NetBehaviour behaviour;

        public NetGUID(Action<NetGUID> eventResponse)
        {
            this.eventResponse = eventResponse;
            GUIDPool.AddGUID(this);
        }

        public NetGUID(int guid)
        {
            this.id = guid;

            GUIDPool.AddToMap(this);
        }

        public void SetGUID(int guid)
        {
            this.id = guid;
            GUIDPool.AddToMap(this);
            eventResponse?.Invoke(this);
        }

        public static void SendEvent(int guid, CEvent e)
        {
            NetBehaviour behaviour = GUIDPool.GetBehaviour(guid);
            if(behaviour != null)
            {
                behaviour.AcceptEvent(e);
            }
        }

        public static void SendEventToServer(int guid, CEvent e)
        {
            NetworkClient.SendObjectToServerTCP(NetworkControlCode.runOnClient, new GUIDBroadcast(guid, e));
        }

        public static void SendEventToServerUdp(int guid, CEvent e)
        {
            NetworkClient.SendObjectToServerUdp(NetworkControlCode.runOnClient, new GUIDBroadcast(guid, e));
        }

        public override string ToString()
        {
            return string.Format("GUID({0})", id);
        }
    }

    [Serializable]
    public class GUIDRequest
    {

    }

    public class ServerGUIDRequest
    {
        public readonly NetConnection connection;

        public ServerGUIDRequest(NetConnection connection)
        {
            this.connection = connection;
        }
    }

    [Serializable]
    public class GUIDResponce
    {
        public int guid;

        public GUIDResponce(int guid)
        {
            this.guid = guid;
        }
    }
}