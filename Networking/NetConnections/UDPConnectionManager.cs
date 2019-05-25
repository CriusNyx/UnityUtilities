//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace UnityUtilities.Networking
//{
//    public static class UdpConnectionManager
//    {
//        static Dictionary<int, UdpClient> clients = new Dictionary<int, UdpClient>();

//        public static (UdpClient client, int actualPort) GetClient(int requestedPort)
//        {
//            if(clients.ContainsKey(requestedPort))
//            {
//                //Debug.Log("Old Client: " + requestedPort);
//                return (clients[requestedPort], requestedPort);
//            }
//            for(int i = requestedPort; i < requestedPort + 10; i++)
//            {
//                if(!CheckPort(i))
//                {
//                    clients[i] = new UdpClient(i);
//                    //Debug.Log("New Client: " + i);
//                    return (clients[i], i);
//                }
//            }
//            return (null, -1);
//        }

//        public static (UdpClient client, int actualPort) GetClient(IPAddress remoteAddress, int requestedPort, int remotePort)
//        {
//            if(clients.ContainsKey(requestedPort))
//            {
//                //Debug.Log("Old Client: " + requestedPort);
//                return (clients[requestedPort], requestedPort);
//            }
//            for(int i = requestedPort; i < requestedPort + 10; i++)
//            {
//                if(!CheckPort(i))
//                {
//                    clients[i] = new UdpClient(i);
//                    clients[i].Connect(new IPEndPoint(remoteAddress, remotePort));
//                    //Debug.Log("New Client: " + i);
//                    return (clients[i], i);
//                }
//            }
//            return (null, -1);
//        }

//        private static bool CheckPort(int portNum)
//        {
//            return System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(p => p.Port == portNum);
//        }
//    }
//}