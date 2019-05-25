using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityUtilities.Networking;

namespace UnityUtilities.Networking
{
    public static class NetTime
    {
        static float offset = 0f;

        static TimeTracker timeTracker = new TimeTracker(0f);

        public static float now
        {
            get
            {
                return timeTracker.Time;
            }
        }

        public static float time
        {
            get
            {
                if(NetworkServer.isServer)
                {
                    return timeTracker.Time;
                }
                return timeTracker.Time + offset;
            }
        }

        public static void Seed(float offset)
        {
            NetTime.offset = offset;
        }
    }
}