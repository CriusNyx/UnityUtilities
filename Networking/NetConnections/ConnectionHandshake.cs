using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityUtilities.Networking;

namespace UnityUtilities.Networking
{
    public static class ConnectionHandshake
    {
        public static void SendServerData(ServerHandshake serverData, NetworkStream stream)
        {
            NetworkInterop.WriteInt(stream, serverData.clientNumber);
        }

        public static ServerHandshake GetServerData(NetworkStream stream)
        {
            int clientNum = NetworkInterop.ReadInt(stream);
            return new ServerHandshake(clientNum);
        }

        public static void SendClientData(ClientHandshake data, NetworkStream stream)
        {
            NetworkInterop.WriteInt(stream, data.receivingPortNumber);
        }

        public static ClientHandshake GetClientData(NetworkStream stream)
        {
            int receivingPortNum = NetworkInterop.ReadInt(stream);
            return new ClientHandshake(receivingPortNum);
        }

        public struct ServerHandshake
        {
            public int clientNumber;

            public ServerHandshake(int clientNumber)
            {
                this.clientNumber = clientNumber;
            }
        }

        public struct ClientHandshake
        {
            public int receivingPortNumber;

            public ClientHandshake(int receivingPortNumber)
            {
                this.receivingPortNumber = receivingPortNumber;
            }
        }

        public static ClientHandshake Handshake(ServerHandshake serverData, NetworkStream stream)
        {
            SendServerData(serverData, stream);
            return GetClientData(stream);
        }

        public static ServerHandshake Handshake(ClientHandshake clientData, NetworkStream stream)
        {
            var output = GetServerData(stream);
            SendClientData(clientData, stream);
            return output;
        }
    }
}