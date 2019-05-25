using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityUtilities.Networking
{
    public class NullNetBehaviour : NetBehaviour
    {
        public override void AcceptEvent(CEvent e)
        {
            //throw new NotImplementedException();
        }

        public override NetBehaviourSpawn GetSpawn()
        {
            return null;
            //throw new NotImplementedException();
        }
    }
}