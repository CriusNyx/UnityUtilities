using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    public class UdpConnection : NetConnection
    {
        UdpConnectionIn inConnection;
        UdpConnectionOut outConnection;

        public UdpConnection(UdpConnectionIn inConnection, UdpConnectionOut outConnection)
        {
            this.inConnection = inConnection;
            this.outConnection = outConnection;
        }

        public override void Dispose()
        {
            inConnection.Dispose();
            outConnection.Dispose();
        }

        public override (NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint) Receive() => inConnection.Receive();

        public override void Send(NetworkControlCode controlCode, byte[] data) => outConnection.Send(controlCode, data);

        public override void Send(NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint) => outConnection.Send(controlCode, data, endPoint);
    }
}