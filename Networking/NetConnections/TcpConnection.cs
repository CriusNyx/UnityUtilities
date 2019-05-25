using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities.Networking
{
    public class TcpConnection : NetConnection
    {
        private TcpClient client;
        private NetworkStream stream;

        public TcpConnection(TcpClient client, NetworkStream stream)
        {
            this.client = client;
            this.stream = stream;
        }

        public override void Dispose()
        {
            stream.Dispose();
            client.Close();
        }

        public override (NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint) Receive()
        {
            byte[] cCode = GetBytes(stream, sizeof(int));
            int controlCode = ToInt(cCode);

            byte[] len = GetBytes(stream, sizeof(int));
            int l = ToInt(len);

            byte[] output = GetBytes(stream, l);

            return ((NetworkControlCode)controlCode, output, null);
        }

        public override void Send(NetworkControlCode controlCode, byte[] data)
        {
            lock(stream)
            {
                byte[] cCode = ToBytes((int)controlCode);
                for(int i = 0; i < cCode.Length; i++)
                {
                    stream.WriteByte(cCode[i]);
                }

                byte[] len = ToBytes(data.Length);
                for(int i = 0; i < cCode.Length; i++)
                {
                    stream.WriteByte(len[i]);
                }

                for(int i = 0; i < data.Length; i++)
                {
                    stream.WriteByte(data[i]);
                }
            }
        }

        public override void Send(NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
    }
}