using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    public class UdpConnectionIn : NetConnection
    {
        UdpClient client;
        //IPAddress localIP, remoteIP;
        //public readonly int localPort, remotePort;

        //public UdpConnectionIn(IPAddress localIP, IPAddress remoteIP, int localPort, int remotePort, bool connect)
        public UdpConnectionIn(UdpClient client)
        {
            this.client = client;
        }

        public override void Dispose()
        {
            client.Close();
        }

        public override (NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint) Receive()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] inData = client.Receive(ref endPoint);
            int controlCode;
            int l;
            byte[] data;
            using(MemoryStream ms = new MemoryStream(inData))
            {
                using(BinaryReader br = new BinaryReader(ms))
                {
                    controlCode = br.ReadInt32();
                    l = br.ReadInt32();
                    data = br.ReadBytes(l);
                }
            }
            return ((NetworkControlCode)controlCode, data, endPoint);
        }

        public override void Send(NetworkControlCode controlCode, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override void Send(NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
    }
}