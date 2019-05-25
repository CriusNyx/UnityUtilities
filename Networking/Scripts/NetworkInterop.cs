using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    public static class NetworkInterop
    {
        public static void WriteObject(NetworkStream stream, NetworkEventType type, object o)
        {
            byte[] byteArr = NetSerialize.Serialize(o);
            WriteArr(stream, type, byteArr);
        }

        public static void WriteArr(NetworkStream stream, NetworkEventType type, byte[] arr)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write((int)type);
            bw.Write(arr.Length);
            bw.Write(arr);
        }

        public static void WriteArr(NetworkStream stream, byte[] arr)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(arr.Length);
            bw.Write(arr);
        }

        public static void WriteInt(NetworkStream stream, int number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            for(int i = 0; i < sizeof(int); i++)
            {
                stream.WriteByte(bytes[i]);
            }
        }

        public static (NetworkEventType eventType, object o) ReadObject(NetworkStream stream)
        {
            NetworkEventType eventType = (NetworkEventType)ReadInt(stream);
            byte[] arr = ReadArr(stream);
            object o = NetSerialize.Deserialize(arr);
            return (eventType, o);
        }

        public static NetworkEventType ReadEventType(NetworkStream stream)
        {
            return (NetworkEventType)ReadInt(stream);
        }

        public static byte[] ReadArr(NetworkStream stream)
        {
            int l = ReadInt(stream);
            return ReadArr(stream, l);
        }

        public static int ReadInt(NetworkStream stream)
        {
            byte[] len = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i++)
            {
                SpinWait.SpinUntil(() => stream.DataAvailable);
                len[i] = (byte)stream.ReadByte();
            }
            return BitConverter.ToInt32(len, 0);
        }

        public static byte[] ReadArr(NetworkStream stream, int len)
        {
            byte[] output = new byte[len];
            for (int i = 0; i < len; i++)
            {
                SpinWait.SpinUntil(() => stream.DataAvailable);
                output[i] = (byte)stream.ReadByte();
            }
            return output;
        }
    }
}
