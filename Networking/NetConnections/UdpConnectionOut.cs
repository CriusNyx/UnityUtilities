using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    public class UdpConnectionOut : NetConnection
    {
        UdpClient client;
        //IPAddress localAddress, remoteAddress;
        //int localPort, remotePort;
        //bool connected;



        //public UdpConnectionOut(IPAddress localAddress, IPAddress remoteAddress, int localPort, int remotePort, bool connected)
        public UdpConnectionOut(UdpClient client)
        {
            this.client = client;

            //this.localAddress = localAddress;
            //this.remoteAddress = remoteAddress;
            //this.localPort = localPort;
            //this.remotePort = remotePort;
            //this.connected = connected;

            //IPEndPoint endPoint = new IPEndPoint(localAddress, localPort);

            //if(connect)
            //{
            //    (client, this.localPort) = UDPConnectionManager.GetClient(remoteAddress, localPort, remotePort);
            //}
            //else
            //{
            //(client, this.localPort) = UdpConnectionManager.GetClient(localPort);
            //client = new UdpClient();
            //}
        }

        public override void Dispose()
        {
            client.Dispose();
        }

        public override (NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint) Receive()
        {
            throw new NotImplementedException();
        }

        public override void Send(NetworkControlCode controlCode, byte[] data)
        {
            //if(debug)
            //{
            //    UnityEngine.Debug.Log("Sending: " + client.Client.RemoteEndPoint.ToString());
            //}
            int l = data.Length + 2 * sizeof(int);
            byte[] output = new byte[l];
            byte[] cCode = ToBytes((int)controlCode);
            byte[] len = ToBytes(data.Length);

            Array.Copy(cCode, 0, output, 0, sizeof(int));
            Array.Copy(len, 0, output, sizeof(int), sizeof(int));
            Array.Copy(data, 0, output, sizeof(int) * 2, data.Length);

            //IPEndPoint endPoint = new IPEndPoint(remoteAddress, remotePort);

            //if(connected)
            //{
            //    client.Send(output, output.Length);
            //}
            //else
            //{
            //    client.Send(output, l, endPoint);
            //}

            //UnityEngine.Debug.Log("Send: " + client.Send(output, l));
            client.Send(output, l);
        }

        public override void Send(NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint)
        {
            int l = data.Length + 2 * sizeof(int);
            byte[] output = new byte[l];
            byte[] cCode = ToBytes((int)controlCode);
            byte[] len = ToBytes(data.Length);

            Array.Copy(cCode, 0, output, 0, sizeof(int));
            Array.Copy(len, 0, output, sizeof(int), sizeof(int));
            Array.Copy(data, 0, output, sizeof(int) * 2, data.Length);

            client.Send(output, l, endPoint);
        }
    }
}