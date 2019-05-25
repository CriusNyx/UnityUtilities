using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    [Serializable]
    public struct PingAssignment
    {
        public int clientNum;
        public float offset;

        public PingAssignment(int clientNum, float offset)
        {
            this.clientNum = clientNum;
            this.offset = offset;
        }
    }
}