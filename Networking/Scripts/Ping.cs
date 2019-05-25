using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    [Serializable]
    public struct Ping
    {
        public float serverTime;
        public float clientTime;
        public int clientNumber;

        public Ping(float serverTime, float clientTime = -1, int clientNumber = -1)
        {
            this.serverTime = serverTime;
            this.clientTime = clientTime;
            this.clientNumber = clientNumber;
        }
    }
}