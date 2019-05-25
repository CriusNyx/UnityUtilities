using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities.Networking
{
    /// <summary>
    /// Class makes decisions about what to do with incoming traffic
    /// </summary>
    public class NetTrafficControl : IDisposable
    {
        /// <summary>
        /// The network connection
        /// </summary>
        NetConnection connection;

        /// <summary>
        /// Event Handler for bytes to run on server
        /// Signature: RunOnServer(int controlCode, byte[] bytes);
        /// </summary>
        Action<NetworkControlCode, byte[], IPEndPoint> RunOnServer;

        /// <summary>
        /// Event Handler for bytes to run on server
        /// Signature: RunOnClient(int controlCode, byte[] bytes);
        /// </summary>
        Action<NetworkControlCode, byte[], IPEndPoint> RunOnClient;

        private Thread readThread;

        public NetTrafficControl(NetConnection connection, Action<NetworkControlCode, byte[], IPEndPoint> RunOnServer, Action<NetworkControlCode, byte[], IPEndPoint> RunOnClient)
        {
            this.connection = connection;
            this.RunOnServer = RunOnServer;
            this.RunOnClient = RunOnClient;

            readThread = new Thread(Listen);
            readThread.Start();
        }

        public void Dispose()
        {
            connection.Dispose();
            readThread.Abort();
        }

        private void Listen()
        {
            try
            {
                while(true)
                {
                    (NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint) = connection.Receive();

                    switch(controlCode)
                    {
                        case NetworkControlCode.runOnServer:
                            RunOnServer(controlCode, data, endPoint);
                            break;
                        case NetworkControlCode.runOnClient:
                            RunOnClient(controlCode, data, endPoint);
                            break;

                    }
                }
            }
            catch(ThreadAbortException)
            {

            }
            catch(ObjectDisposedException)
            {

            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    public enum NetworkControlCode
    {
        runOnServer = 0,
        runOnClient = 1
    }
}