using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities.Networking
{
    public class NetSerialize
    {
        static BinaryFormatter formatter = new BinaryFormatter();

        public static byte[] Serialize(object o)
        {
            byte[] output;

            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, o);

                output = stream.ToArray();
            }
            output = Compression.Compress(output);

            return output;
        }

        public static object Deserialize(byte[] arr)
        {
            var dearr = Compression.Decompress(arr);
            object o;

            using (MemoryStream stream = new MemoryStream(dearr))
            {
                o = formatter.Deserialize(stream);
            }

            return o;
        }
    }

    public static class Compression
    {
        public static byte[] Compress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal))
                {
                    gzip.Write(data, 0, data.Length);
                }
                data = ms.ToArray();
            }
            return data;
        }
        public static byte[] Decompress(byte[] data)
        {
            // the trick is to read the last 4 bytes to get the length
            // gzip appends this to the array when compressing
            var lengthBuffer = new byte[4];
            Array.Copy(data, data.Length - 4, lengthBuffer, 0, 4);
            int uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);
            var buffer = new byte[uncompressedSize];
            using (var ms = new MemoryStream(data))
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    gzip.Read(buffer, 0, uncompressedSize);
                }
            }
            return buffer;
        }
    }
}