using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    [Serializable]
    public class GUIDBroadcast
    {
        public readonly int guid;
        public readonly CEvent e;

        public GUIDBroadcast(int guid, CEvent e)
        {
            this.guid = guid;
            this.e = e;
        }

        public override string ToString()
        {
            return string.Format("GUIDBroadcast({0}, {1})", guid, e.ToString());
        }
    }
}