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
using UnityUtilities;

namespace UnityUtilities.Networking
{
    /// <summary>
    /// A network server to manage client connections, and broadcast network events.
    /// </summary>
    public class NetworkServer : MonoBehaviour
    {
        #region Fields and Properties
        //---------------------------------                Ports                              ----------------------------------------
        public const int
            TCP_PORT = 18550,
            UDP_PORT = TCP_PORT;

        /// <summary>
        /// Retruns true if this program is the server
        /// </summary>
        public static bool isServer { get; private set; } = false;






        //---------------------------------                Client Info                              ----------------------------------------


        /// <summary>
        /// The list of connected IPS
        /// </summary>
        private List<string> privateIPList = new List<string>();

        /// <summary>
        /// The list of connected IPS
        /// </summary>
        public IEnumerable<string> IPList
        {
            get
            {
                return privateIPList;
            }
        }

        public object LogFiles { get; private set; }

        /// <summary>
        /// The current list of clients
        /// </summary>
        List<ClientInfo> clientList = new List<ClientInfo>();

        /// <summary>
        /// The ping times for the client
        /// </summary>
        Dictionary<int, (float, float, float, float)> clientPingTimes = new Dictionary<int, (float, float, float, float)>();

        /// <summary>
        /// Lock for determining client numbers
        /// </summary>
        private object clientNumberLock = new object();

        /// <summary>
        /// The current client number to assign
        /// </summary>
        private int currentClientNumber = 0;







        /// <summary>
        /// The current list of open threads
        /// </summary>
        List<Thread> threads = new List<Thread>();







        //---------------------------------                TCP                              ----------------------------------------


        /// <summary>
        /// A list of TCP Connections
        /// </summary>
        List<NetConnection> tcpConnections = new List<NetConnection>();

        /// <summary>
        /// A list of tcp connectsion for current clients
        /// </summary>
        Dictionary<int, NetConnection> clientNetConnections = new Dictionary<int, NetConnection>();







        //---------------------------------                UDP                              ----------------------------------------


        /// <summary>
        /// Net connection for UDP traffic
        /// </summary>
        NetConnection udp;

        /// <summary>
        /// A list of UDP End Points
        /// </summary>
        List<IPEndPoint> endPoints = new List<IPEndPoint>();







        //---------------------------------                Traffic Control                              ----------------------------------------


        /// <summary>
        /// Traffic controlers for incoming net traffic
        /// </summary>
        List<NetTrafficControl> trafficControllers = new List<NetTrafficControl>();


        Queue<object> objectQueue = new Queue<object>();


#if UNITY_EDITOR
        public int
            maxObjectCount = 500;

        Queue<string>
            incomingObjectList = new Queue<string>();

        public IEnumerable<string>
            IncomingObjectList()
        {
            lock(incomingObjectList)
            {
                foreach(var o in incomingObjectList)
                {
                    yield return o;
                }
            }
        }

        Queue<string>
            outgoingObjectList = new Queue<string>();





        public IEnumerable<string> OutgoingObjectList()
        {
            lock(outgoingObjectList)
            {
                foreach(var o in outgoingObjectList)
                {
                    yield return o;
                }
            }
        }

        public void AddToObjectList(string s, Queue<string> queue)
        {
            lock(queue)
            {
                queue.Enqueue(s);
                if(queue.Count > maxObjectCount)
                {
                    queue.Dequeue();
                }
            }
        }
#endif
        #endregion








        #region Getters
        /// <summary>
        /// Get a list of client ping times
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(int, float, float, float, float)> GetClientPingTimes()
        {
            lock(clientPingTimes)
            {
                foreach(var pair in clientPingTimes)
                {
                    int clientNumber = pair.Key;
                    (float serverTime, float clientTime, float rtt, float offset) = pair.Value;
                    yield return (clientNumber, serverTime, clientTime, rtt, offset);
                }
            }
        }




        /// <summary>
        /// Get a list of clients
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ClientInfo> GetClientList()
        {
            lock(clientList)
            {
                foreach(var info in clientList)
                {
                    yield return info;
                }
            }
        }
        #endregion








        #region MonoBehaviour
        /// <summary>
        /// Initialize the NetworkServer
        /// </summary>
        private void Awake()
        {
            LogFile.WriteLineToLog(LogFile.FileNames.server, "Server Started");
            isServer = true;
            InitializeTCP();
            InitializeUDP();
        }


        /// <summary>
        /// Creates TCP listeners, and open ports for TCP
        /// </summary>
        private void InitializeTCP()
        {
            LogFile.WriteLineToLog(LogFile.FileNames.server, "Initializing TCP");

            //bind a TCPListener to each IP on this machine
            string hostName = Dns.GetHostName();

            IPAddress[] thisMachinesAddress = Dns.GetHostEntry(hostName).AddressList;

            //create a thread for each listener to watch incoming connections
            lock(threads)
            {
                foreach(var addr in thisMachinesAddress)
                {
                    //create a new listener for that thread
                    TcpListener listener = new TcpListener(addr, TCP_PORT);

                    //start listener thread
                    var thread = new Thread(() => ListenLoop(listener));
                    thread.Start();
                    threads.Add(thread);

                    LogFile.WriteLineToLog(LogFile.FileNames.server, "Started TCP Listener for Address " + addr + ":" + TCP_PORT);
                }
            }

            //Add addressess to address list for debugging
            foreach(var address in thisMachinesAddress)
            {
                privateIPList.Add(address.ToString());
            }

            //make listener for loopback address
            {
                //create a new listener for that thread
                TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), TCP_PORT);

                //start listener thread
                var thread = new Thread(() => ListenLoop(listener));
                thread.Start();
                threads.Add(thread);

                LogFile.WriteLineToLog(LogFile.FileNames.server, "Started TCP Listener for Address 127.0.0.1:" + TCP_PORT);
            }

            privateIPList.Add("127.0.0.1");
        }


        /// <summary>
        /// Create UDP client for UDP traffic, and initialize a UDP traffic controller
        /// </summary>
        private void InitializeUDP()
        {
            //create udp in thread
            UdpClient inClient = new UdpClient(UDP_PORT);
            UdpConnectionIn udpIn = new UdpConnectionIn(inClient);
            UdpConnectionOut udpOut = new UdpConnectionOut(inClient);
            udp = new UdpConnection(udpIn, udpOut);

            NetTrafficControl udpControl = new NetTrafficControl(udpIn, (x, y, z) => RunOnServer(x, y, null, z), (x, y, z) => RunOnClient(x, y, null));

            LogFile.WriteLinesToLog(LogFile.FileNames.server, new string[] {
                "Started UDP Listener",
                "   Local End Point:"  + inClient?.Client?.LocalEndPoint?.ToString(),
                "   Remote End Point:"  + inClient?.Client?.RemoteEndPoint?.ToString(),
            });
        }





        /// <summary>
        /// Update the network server, to execute logic on frame
        /// </summary>
        private void Update()
        {
            List<string> logLines = new List<string>();

            //dequeue each object, to process it
            lock(objectQueue)
            {
                while(objectQueue.Count > 0)
                {
                    object o = objectQueue.Dequeue();
                    ProcessObject(o);
                    logLines.Add("Processed Object: " + o.ToString());
                }
            }

            if(logLines.Count > 0)
                LogFile.WriteLinesToLog(LogFile.FileNames.serverObjectLog, logLines);

            //Send out pings to clients for calculating ping times
            if(Time.frameCount % 100 == 0)
            {
                List<string> currentEndPoints = new List<string>();
                currentEndPoints.Add("Pinging Clients");

                float time = NetTime.now;

                lock(endPoints)
                {
                    foreach(var ep in endPoints)
                    {
                        udp.Send(NetworkControlCode.runOnClient, NetSerialize.Serialize(new Ping(NetTime.now)), ep);
                        currentEndPoints.Add("   " + ep.ToString());
                    }
                }

                LogFile.WriteLinesToLog(LogFile.FileNames.serverObjectLog, currentEndPoints);
            }
        }


        /// <summary>
        /// Process an object from the object queue
        /// </summary>
        /// <param name="o"></param>
        private void ProcessObject(object o)
        {
            //Broadcast the network event across the event system
            if(o is NetworkEventBroadcast ne)
            {
                ProcessNetworkEventBroadcast(o, ne);
            }

            //If this is a guid request, process the request, and return a response
            else if(o is ServerGUIDRequest request)
            {
                ProcessServerGUIDRequest(request);
            }

            //If this is a ping assignment, broadcast it to the desired client.
            else if(o is PingAssignment pa)
            {
                ProcessPingAssignment(pa);
            }
        }


        /// <summary>
        /// Broadcast the network event that are intended for the server
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ne"></param>
        private void ProcessNetworkEventBroadcast(object o, NetworkEventBroadcast ne)
        {
            CEventSystem.Broadcast(ne.channel, ne.subchannel, ne.e);

#if UNITY_EDITOR
            AddToObjectList(o.ToString(), outgoingObjectList);
#endif
        }


        /// <summary>
        /// Process a GUID request, and send the output to the client
        /// </summary>
        /// <param name="request"></param>
        private void ProcessServerGUIDRequest(ServerGUIDRequest request)
        {
            int guid = GUIDPool.GetGUID();
            request.connection.Send(NetworkControlCode.runOnClient, NetSerialize.Serialize(new GUIDResponce(guid)));

#if UNITY_EDITOR
            AddToObjectList("GUID Response: " + guid, outgoingObjectList);
#endif
        }


        /// <summary>
        /// Send out ping assignment to client
        /// </summary>
        /// <param name="pa"></param>
        private void ProcessPingAssignment(PingAssignment pa)
        {
            lock(clientNetConnections)
            {
                NetConnection connection = clientNetConnections[pa.clientNum];
                connection.Send(NetworkControlCode.runOnClient, NetSerialize.Serialize(pa));
            }
        }





        /// <summary>
        /// On destruction, destroy all clients and traffic controllers
        /// </summary>
        private void OnDestroy()
        {
            lock(trafficControllers)
            {
                lock(tcpConnections)
                {
                    foreach(var controller in trafficControllers)
                    {
                        try
                        {
                            controller.Dispose();
                        }
                        catch(Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    foreach(var tcp in tcpConnections)
                    {
                        try
                        {
                            tcp.Dispose();
                        }
                        catch(Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    foreach(var thread in threads)
                    {
                        thread.Abort();
                    }
                }
            }
        }
        #endregion

        #region Incoming Connection Management
        /// <summary>
        /// Monitors incoming connections and creates new clients whenever a request is received
        /// </summary>
        /// <param name="listener"></param>
        private void ListenLoop(TcpListener listener)
        {
            //start the listener
            listener.Start();

            try
            {
                while(true)
                {
                    // Get a client from the listener, and block until one is reached
                    TcpClient client = listener.AcceptTcpClient();

                    LogFile.WriteLinesToLog(LogFile.FileNames.server,
                        new string[] {
                            "TCP Client connected",
                            "   Local End Point: " + client?.Client?.LocalEndPoint?.ToString(),
                            "   Remote End Point: " + client?.Client?.RemoteEndPoint?.ToString() });

                    ConnectToClient(client);
                }
            }

            // Closes listener on thread abort without throwing an exception
            catch(Exception e)
            {
                if(!(e is ThreadAbortException))
                {
                    Debug.LogError("Error when listening for TCP Clients: ");
                    Debug.LogError(e);

                    LogFile.WriteLinesToLog(LogFile.FileNames.server, new string[]
                    {
                        "Error when listening for TCP Clients: ",
                        e.ToString()
                            });
                }
            }

            listener.Stop();
        }


        /// <summary>
        /// Accept an incoming tcp connection, and initialize the network client
        /// </summary>
        /// <param name="client"></param>
        private void ConnectToClient(TcpClient client)
        {
            // Establish tcp connectionclientNumber

            // Get a network stream from the client
            NetworkStream stream = client.GetStream();

            int clientNumber = GetClientNumber();

            LogFile.WriteLineToLog(LogFile.FileNames.server, "TCP Client Number issued: " + clientNumber);

            // Preform handshake
            NetworkInterop.WriteInt(stream, clientNumber);

            // Create TCP Connection
            TcpConnection tcp = new TcpConnection(client, stream); ;
            lock(tcpConnections)
            {
                lock(clientNetConnections)
                {
                    tcpConnections.Add(tcp);
                    clientNetConnections.Add(clientNumber, tcp);
                }
            }

            LogFile.WriteLineToLog(LogFile.FileNames.server, "TCP Connection Initialized: " + clientNumber);

            SyncClient(tcp, clientNumber);

            // Get endpoints for creating udp connections
            IPEndPoint localEndPoint = (IPEndPoint)client.Client.LocalEndPoint;
            IPAddress localAddress = localEndPoint.Address;

            IPEndPoint remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            IPAddress clientAddress = remoteEndPoint.Address;

            // Create a net traffic controller for incoming tcp traffic
            lock(trafficControllers)
            {
                NetTrafficControl tcpTrafficControl = new NetTrafficControl(tcp, (x, y, z) => RunOnServer(x, y, tcp, z), (x, y, z) => RunOnClient(x, y, tcpConnections));
                trafficControllers.Add(tcpTrafficControl);
            }

            LogFile.WriteLineToLog(LogFile.FileNames.server, "TCP Traffic Controller Initialized: " + clientNumber);

            // Add the clients info to the client list for debugging
            lock(clientList)
            {
                clientList.Add(new ClientInfo(clientNumber, clientAddress.ToString(), -1));
            }

            LogFile.WriteLineToLog(LogFile.FileNames.server, "TCP Client Added To List: " + clientNumber);
        }

        public void SyncClient(NetConnection tcp, int clientNumber)
        {
            List<string> lines = new List<string>();
            lines.Add("Syncing Client: " + clientNumber);

            foreach(NetBehaviour net in NetBehaviourTracker.GetNetBehaviours())
            {
                lines.Add("Getting Net Spawn: " + net.GetType().ToString());
                var spawn = net.GetSpawn();
                if(spawn != null)
                {
                    lines.Add("Sending Spawn Request: " + spawn.ToString());
                    tcp.Send(
                        NetworkControlCode.runOnClient,
                        NetSerialize.Serialize(
                            new NetworkEventBroadcast(
                                NetBehaviourSpawner.Channel.channel,
                                NetBehaviourSpawner.Channel.subchannel,
                                spawn)));
                }
                else
                {
                    lines.Add("Null Spawn Request");
                }
            }

            LogFile.WriteLinesToLog(LogFile.FileNames.server, lines);
        }

        /// <summary>
        /// Get a client number for the new client
        /// </summary>
        /// <returns></returns>
        private int GetClientNumber()
        {
            int clientNumber;
            lock(clientNumberLock)
            {
                clientNumber = currentClientNumber;
                currentClientNumber++;
            }

            LogFile.WriteLineToLog(LogFile.FileNames.server, "Client Number Fetched: " + clientNumber);

            return clientNumber;
        }
        #endregion

        #region Incoming Object Mangement
        [Serializable]
        public class UdpConnectServerEvent
        {

        }

        /// <summary>
        /// Accept an incoming object designated to run on the server
        /// </summary>
        /// <param name="controlCode"></param>
        /// <param name="data"></param>
        /// <param name="connection"></param>
        private void RunOnServer(NetworkControlCode controlCode, byte[] data, NetConnection connection, IPEndPoint endPoint)
        {
            // Get the object code
            object o = NetSerialize.Deserialize(data);

            // Special case for a ping.
            // This special case needs to be processed here to reduce ping response times, and increase clock syncronization accuracy
            if(o is Ping p)
            {
                LogFile.WriteLineToLog(LogFile.FileNames.server, "Ping Returned from Client: " + endPoint.ToString() + ", clientNumber: " + p.clientNumber + ", " + p.ToString());

                // Get data needed to process ping, and calculate clock sync
                float time = NetTime.now;
                float rtt = time - p.serverTime;
                float lag = rtt / 2f;
                float timeClinetNow = p.clientTime + lag;
                float offset = time - timeClinetNow;

                // Create a ping assignment, and add it to the object queue
                QueueObject(new PingAssignment(p.clientNumber, offset), connection);

                // Add ping info to the server debug log
                lock(clientPingTimes)
                {
                    clientPingTimes[p.clientNumber] = (p.serverTime, p.clientTime, rtt, offset);
                }
            }
            else if(o is UdpConnectServerEvent)
            {
                LogFile.WriteLineToLog(LogFile.FileNames.server, "UDP Connection received from client: " + endPoint.ToString());
                lock(endPoints)
                {
                    endPoints.Add(endPoint);
                }
            }
            else
            {
                QueueObject(o, connection);
            }
        }

        /// <summary>
        /// Accept an object designated to run on the client
        /// </summary>
        /// <param name="controlCode"></param>
        /// <param name="data"></param>
        /// <param name="connections"></param>
        private void RunOnClient(NetworkControlCode controlCode, byte[] data, List<NetConnection> connections)
        {
            // This code does no error checking on the data, and may be succeptible to DoS attacks.

            // NOTES:
            // Any malicious client could connect officially, and then overwhelm the server with garbage, and potentially preform code injection on clients
            // Check if c# has any protections on deserializing malicious data, or code injection protections
            // Also, consider writing a protocol for disconnecting malicious clients, or clients that send malformed objects

            //broadcast all objects to all clients

            List<string> lines = new List<string>();
            lines.Add("Received Object to Run on Client");

            if(connections != null)
            {
                lines.Add("Object is TCP");
                lock(connections)
                {
                    foreach(var connection in connections)
                    {
                        connection.Send(controlCode, data);
                    }
                }
            }
            else
            {
                lines.Add("Object is UDP");
                lock(endPoints)
                {
                    foreach(var endPoint in endPoints)
                    {
                        lines.Add("Sending Object to EP: " + endPoint);
                        udp.Send(controlCode, data, endPoint);
                    }
                }
            }

#if UNITY_EDITOR
            object o = NetSerialize.Deserialize(data);

            AddToObjectList("Client: " + o.ToString(), incomingObjectList);

            lines.Add("Object is " + o.ToString());
#endif

            LogFile.WriteLinesToLog(LogFile.FileNames.serverObjectLog, lines);
        }

        /// <summary>
        /// Queue an object for on frame processing
        /// </summary>
        /// <param name="o"></param>
        /// <param name="connection"></param>
        private void QueueObject(object o, NetConnection connection)
        {
#if UNITY_EDITOR
            // Add an object to the debug list for incoming objects
            AddToObjectList("Server: " + o.ToString(), incomingObjectList);

            LogFile.WriteLineToLog(LogFile.FileNames.serverObjectLog, "Object Queued for On Frame: " + o.ToString());
#endif
            // If this is a network event, disable to send to server flag to prevent infinate feedback loops
            if(o is NetworkEventBroadcast ne)
            {
                ne.e.sendToServer = false;
            }
            // Add the object to the object queue for on frame processing
            lock(objectQueue)
            {
                // If this object is a GUIDRequest, then transform it into a server request
                // Consider refactoring this to get GUID Requests with less lag
                if(o is GUIDRequest)
                {
                    objectQueue.Enqueue(new ServerGUIDRequest(connection));
                }
                else
                {
                    objectQueue.Enqueue(o);
                }
            }
        }
        #endregion

        #region Debug
        /// <summary>
        /// Converts a byte array to a string for debugging
        /// This can be used to check for data errors, or data loss
        /// </summary>
        /// <param name="ba"></param>
        /// <returns></returns>
        private static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }
        #endregion

        public struct ClientInfo
        {
            public readonly int clientNumber;
            public readonly string ip;
            public readonly int port;

            public ClientInfo(int clientNumber, string ip, int port)
            {
                this.clientNumber = clientNumber;
                this.ip = ip;
                this.port = port;
            }

            public override string ToString()
            {
                return string.Format("clientNumber: {0} clientIP: {1}, clientPort: {2}", clientNumber, ip, port);
            }
        }
    }

    public enum NetworkEventType
    {
        runOnServer = 0,
        runOnClient = 1
    }

    public enum ServerEventChannel
    {
        channel,
        subchannel
    }
}
