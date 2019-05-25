using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityUtilities;

namespace UnityUtilities.Networking
{
    public class NetworkControllerTest : MonoBehaviour
    {
        const string IP = "192.168.1.84";
        const int PORT = 18550;

        public object TCPListener { get; private set; }

        public void AcceptEvent(CEvent e)
        {
            if(e is TestEvent te)
            {
                Debug.Log(te.text);
            }
        }

        private void Start()
        {
            gameObject.AddComponent<NetworkServer>();
            NetworkClient.Create(gameObject, IP);

            gameObject.AddComponent<ServerDebugListener>();

            CEventSystem.Broadcast(ServerEventChannel.channel, ServerEventChannel.subchannel, new ServerDebugListener.ServerDebugEvent("This is a server debug event"));
            CEventSystem.Broadcast(ServerEventChannel.channel, ServerEventChannel.subchannel, new ServerDebugListener.ClientDebugEvent("This is a client debug event"));
        }        
    }

    [System.Serializable]
    public class Vector3Wrapper
    {
        public float x, y, z;

        public Vector3Wrapper(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vector3Wrapper(Vector3 v) => new Vector3Wrapper(v.x, v.y, v.z);
        public static implicit operator Vector3(Vector3Wrapper v) => new Vector3(v.x, v.y, v.z);

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
    }

    [Serializable]
    public class Vector2Wrapper
    {
        public float x, y;

        public Vector2Wrapper(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2Wrapper(Vector2 v) => new Vector2Wrapper(v.x, v.y);
        public static implicit operator Vector2 (Vector2Wrapper v) => new Vector2(v.x, v.y);

        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }
    }

    [Serializable]
    public class QuaternionWrapper
    {
        public float x, y, z, w;

        public QuaternionWrapper(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator QuaternionWrapper(Quaternion q) => new QuaternionWrapper(q.x, q.y, q.z, q.w);
        public static implicit operator Quaternion(QuaternionWrapper q) => new Quaternion(q.x, q.y, q.z, q.w);
    }

    [Serializable]
    public class TestEvent : NetworkedCEvent
    {
        public readonly string text;

        public TestEvent(string text)
        {
            this.text = text;
        }
    }

    public enum TestEventChannel
    {
        channel,
        subchannel
    }
}