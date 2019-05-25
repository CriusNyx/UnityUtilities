using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities.Networking
{
    public class NetworkClient : MonoBehaviour
    {
        #region Fields and Properties

        /// <summary>
        /// Current Network Client Instance
        /// </summary>
        static NetworkClient instance;


        /// <summary>
        /// Connection to the server for the specified protocols
        /// </summary>
        NetConnection
            tcp,
            udp;


        /// <summary>
        /// Traffic controllers for incoming net traffic direction and management
        /// </summary>
        NetTrafficControl
            tcpTrafficControl,
            udpTrafficControl;


        /// <summary>
        /// The current number of this client
        /// </summary>
        private int clientNumber;


        /// <summary>
        /// The current number of this client
        /// </summary>
        public static int ClientNumber
        {
            get
            {
                if(instance != null)
                {
                    return instance.clientNumber;
                }
                else
                {
                    return -1;
                }
            }
        }


        /// <summary>
        /// A queue of objects to be processed on frame
        /// </summary>
        Queue<object> objectQueue = new Queue<object>();

        #endregion

        #region Editor Code

#if UNITY_EDITOR
        /// <summary>
        /// Maximum number of objects in queue for debugging
        /// </summary>
        public int maxQueueCount = 500;


        /// <summary>
        /// Object queue for debugging
        /// </summary>
        Queue<string> objectList = new Queue<string>();





        /// <summary>
        /// Get the Editor Object list for debugging
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ObjectList()
        {
            lock(objectList)
            {
                foreach(var o in objectList)
                {
                    yield return o;
                }
            }
        }


        /// <summary>
        /// Add an object to the editor object list
        /// </summary>
        /// <param name="s"></param>
        public void AddToObjectList(string s)
        {
            lock(objectList)
            {
                objectList.Enqueue(s);
                if(objectList.Count > maxQueueCount)
                {
                    objectList.Dequeue();
                }
            }
        }
#endif
        #endregion

        #region MonoBehaviour

        /// <summary>
        /// Create a new network controller that connects to the serverIP
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="serverIP"></param>
        /// <returns></returns>
        public static NetworkClient Create(GameObject gameObject, string serverIP)
        {
            return gameObject.AddComponent<NetworkClient>()._Create(serverIP);
        }


        /// <summary>
        /// Creates a new network controllers, initializes TCP/UDP connections, and preforms state synchronization with the server
        /// </summary>
        /// <param name="serverIP"></param>
        /// <returns></returns>
        private NetworkClient _Create(string serverIP)
        {
            LogFile.WriteLineToLog(LogFile.FileNames.client, "Client Started: " + serverIP);

            instance = this;

            TcpClient client = InitializeTCPClient(serverIP);

            UdpClient udpClient = InitializeUDPClient(serverIP);

            CreateNetTrafficControllers(client, udpClient);

            return this;
        }


        /// <summary>
        /// Create the TCP client and preform TCP initialization.
        /// </summary>
        /// <param name="serverIP"></param>
        /// <returns></returns>
        private TcpClient InitializeTCPClient(string serverIP)
        {
            LogFile.WriteLineToLog(LogFile.FileNames.client, "TCP Connecting");
            //get client and stream
            TcpClient client = new TcpClient(serverIP, NetworkServer.TCP_PORT);
            NetworkStream stream = client.GetStream();

            LogFile.WriteLinesToLog(LogFile.FileNames.client, new string[]{
            "TCP Connected",
            "Local End Point: " + client.Client.LocalEndPoint.ToString(),
            "Remote End Point: " + client.Client.RemoteEndPoint.ToString()}
            );

            clientNumber = NetworkInterop.ReadInt(stream);

            LogFile.WriteLineToLog(LogFile.FileNames.client, "Received Client Number: " + clientNumber);

            //create tcp connection
            tcp = new TcpConnection(client, stream);
            return client;
        }


        /// <summary>
        /// Initialize the UDP client, and send connection request to the server
        /// </summary>
        /// <param name="serverIP"></param>
        /// <returns></returns>
        private UdpClient InitializeUDPClient(string serverIP)
        {
            LogFile.WriteLineToLog(LogFile.FileNames.client, "UDP Connecting");
            //create udp listener to get port
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(serverIP, NetworkServer.UDP_PORT);

            LogFile.WriteLinesToLog(LogFile.FileNames.client, new string[]{
            "UDP Connected",
            "Local End Point: " + udpClient.Client.LocalEndPoint.ToString(),
            "Remote End Point: " + udpClient.Client.RemoteEndPoint.ToString()}
            );

            UdpConnectionIn udpIn = new UdpConnectionIn(udpClient);

            //create UDPOut connection
            UdpConnectionOut udpOut = new UdpConnectionOut(udpClient);

            //Encapsulate udp connections
            udp = new UdpConnection(udpIn, udpOut);

            udp.Send(NetworkControlCode.runOnServer, NetSerialize.Serialize(new NetworkServer.UdpConnectServerEvent()));

            LogFile.WriteLineToLog(LogFile.FileNames.client, "UDP Connection Request Sent");
            return udpClient;
        }


        /// <summary>
        /// Create a traffic controllers for the TCP/UDP client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="udpClient"></param>
        private void CreateNetTrafficControllers(TcpClient client, UdpClient udpClient)
        {
            tcpTrafficControl = new NetTrafficControl(tcp, (x, y, z) => { }, (x, y, z) => RunOnClient(x, y));
            udpTrafficControl = new NetTrafficControl(udp, (x, y, z) => { }, (x, y, z) => RunOnClient(x, y));

            LogFile.WriteLinesToLog(LogFile.FileNames.client, new string[] {
                "Traffic Controllers Created:",
                "TCP",
                "   Local End Point: " + client.Client.LocalEndPoint.ToString(),
                "   Remote End Point: " + client.Client.RemoteEndPoint.ToString(),
                "UDP",
                "   Local End Point: " + udpClient.Client.LocalEndPoint.ToString(),
                "   Remote End Point: " + udpClient.Client.RemoteEndPoint.ToString(),
            });
        }









        /// <summary>
        /// Process all queued objects on frame.
        /// </summary>
        private void Update()
        {
            //Process the objects queue
            lock(objectQueue)
            {
                List<string> lines = new List<string>();

                while(objectQueue.Count > 0)
                {
                    object o = objectQueue.Dequeue();

                    lines.Add("Object Processed: " + o.ToString());
                    ProcessObject(o);
                }
                if(lines.Count > 0)
                {
                    LogFile.WriteLinesToLog(LogFile.FileNames.clientObjectLog, lines);
                }
            }
        }


        /// <summary>
        /// Detect objects type, and process it accordingly.
        /// </summary>
        /// <param name="o"></param>
        private static void ProcessObject(object o)
        {
            if(o is NetworkEventBroadcast netEvent)
            {
                ProcessNetworkEventBroadcast(netEvent);
            }
            if(o is GUIDBroadcast gb)
            {
                ProcessGUIDBroadcast(gb);
            }
            if(o is GUIDResponce resp)
            {
                ProcessGUIResponse(resp);
            }
            if(o is PingAssignment p)
            {
                ProcessNetTimeAssignment(p);
            }
            if(o is NetworkState.NetworkStateSync sync)
            {
                ProcessNetworkStateSync(sync);
            }
        }
    

        /// <summary>
        /// Process a network event broadcast, and push it into the event system
        /// </summary>
        /// <param name="netEvent"></param>
        private static void ProcessNetworkEventBroadcast(NetworkEventBroadcast netEvent)
        {
            Enum channel = netEvent.channel, subchannel = netEvent.subchannel;
            NetworkedCEvent e = netEvent.e;

            e.sendToServer = false;
            CEventSystem.Broadcast(channel, subchannel, e);
        }

        
        /// <summary>
        /// Send a GUID broadcast to the appropriate object
        /// </summary>
        /// <param name="gb"></param>
        private static void ProcessGUIDBroadcast(GUIDBroadcast gb)
        {
            NetGUID.SendEvent(gb.guid, gb.e);
        }


        /// <summary>
        /// Accept the GUID response
        /// </summary>
        /// <param name="resp"></param>
        private static void ProcessGUIResponse(GUIDResponce resp)
        {
            GUIDPool.AcceptGUID(resp.guid);
        }


        /// <summary>
        /// Process a net ping assignment, and sync clock
        /// </summary>
        /// <param name="p"></param>
        private static void ProcessNetTimeAssignment(PingAssignment p)
        {
            NetTime.Seed(p.offset);
        }


        /// <summary>
        /// Process a network state sync for an object
        /// </summary>
        /// <param name="sync"></param>
        private static void ProcessNetworkStateSync(NetworkState.NetworkStateSync sync)
        {
            NetworkState.ApplySync(sync);
        }




        /// <summary>
        /// Accept an object from the server to be run on the client
        /// </summary>
        /// <param name="controlCode"></param>
        /// <param name="bytes"></param>
        private void RunOnClient(NetworkControlCode controlCode, byte[] bytes)
        {
            object o = NetSerialize.Deserialize(bytes);

            //If object is a ping, respond immediately, to minimize lag
            //This should be replaced with raw data and custom control codes that are faster to deserialize to help minimize lag from object deserialization
            if(o is Ping p)
            {
                float time = NetTime.now;
                p.clientNumber = clientNumber;
                p.clientTime = time;
                udp.Send(NetworkControlCode.runOnServer, NetSerialize.Serialize(p));
            }
            else
            {
                lock(objectQueue)
                {
                    objectQueue.Enqueue(o);
                }

#if UNITY_EDITOR
                AddToObjectList("FromServer: " + o.ToString());
#endif
            }
        }







        public void OnGUI()
        {
            GUILayout.Label("Time: " + NetTime.time);
        }


        private void OnDestroy()
        {
            instance = null;
            udpTrafficControl.Dispose();
            tcpTrafficControl.Dispose();
        }
        #endregion







        #region Send Object to Server

        /// <summary>
        /// Serialize an object and send it to the server over TCP
        /// </summary>
        /// <param name="controlCode"></param>
        /// <param name="o"></param>
        public static void SendObjectToServerTCP(NetworkControlCode controlCode, object o)
        {
            instance?._SendObjectToServerTCP(controlCode, o);
        }


        /// <summary>
        /// Serialize an object and send it to the server over TCP
        /// </summary>
        /// <param name="controlCode"></param>
        /// <param name="o"></param>
        private void _SendObjectToServerTCP(NetworkControlCode controlCode, object o)
        {
            LogFile.WriteLineToLog(LogFile.FileNames.clientObjectLog, "Sending Object To Server TCP: " + o.GetType().ToString());

#if UNITY_EDITOR
            objectList.Enqueue("ToServer: " + o.ToString());
#endif
            byte[] bytes = NetSerialize.Serialize(o);
            instance.tcp.Send(controlCode, bytes);
        }





        /// <summary>
        /// Send an object to the server over UDP
        /// </summary>
        /// <param name="controlCode"></param>
        /// <param name="o"></param>
        public static void SendObjectToServerUdp(NetworkControlCode controlCode, object o)
        {
            instance?._SendObjectToServerUdp(controlCode, o);
        }


        /// <summary>
        /// Send an object to the server over UDP
        /// </summary>
        /// <param name="controlCode"></param>
        /// <param name="o"></param>
        public void _SendObjectToServerUdp(NetworkControlCode controlCode, object o)
        {
            LogFile.WriteLineToLog(LogFile.FileNames.clientObjectLog, "Sending Object To Server UDP: " + o.GetType().ToString());

#if UNITY_EDITOR
            lock(objectList)
            {
                AddToObjectList("ToServerUdp: " + o.ToString());
            }
#endif

            byte[] arr = NetSerialize.Serialize(o);
            instance.udp.Send(controlCode, arr);
        }
        #endregion
    }
}