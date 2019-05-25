using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    public abstract class NetConnection : IDisposable
    {
        public abstract void Send(NetworkControlCode controlCode, byte[] data);
        public abstract void Send(NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint);

        public abstract (NetworkControlCode controlCode, byte[] data, IPEndPoint endPoint) Receive();

        protected static byte[] GetBytes(NetworkStream stream, int len)
        {
            byte[] output = new byte[len];

            for(int i = 0; i < len; i++)
            {
                SpinWait.SpinUntil(() => stream.DataAvailable);
                output[i] = (byte)stream.ReadByte();
            }

            return output;
        }

        protected int ToInt(byte[] arr) => BitConverter.ToInt32(arr, 0);

        protected byte[] ToBytes(int i) => BitConverter.GetBytes(i);

        public abstract void Dispose();
    }
}