using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities.Networking
{
    [ExecuteInEditMode]
    public class ReservedGUID : MonoBehaviour
    {
        public int id = -1;

        private void Awake()
        {
            ProcessGUID();
        }

        private void Update()
        {
            ProcessGUID();
        }

        private void ProcessGUID()
        {
            if(id < 0)
            {
                int startID = 0;
                Dictionary<int, ReservedGUID> guids = new Dictionary<int, ReservedGUID>();
                List<ReservedGUID> unprocessedIDS = new List<ReservedGUID>();

                foreach(ReservedGUID rguid in FindObjectsOfType<ReservedGUID>())
                {
                    int otherGUID = rguid.id;
                    startID = Math.Max(otherGUID + 1, startID);
                    if(otherGUID != -1)
                    {
                        if(guids.ContainsKey(otherGUID))
                        {
                            rguid.id = -1;
                        }
                        else
                        {
                            guids[otherGUID] = rguid;
                        }
                    }

                    otherGUID = rguid.id;
                    if(otherGUID == -1)
                    {
                        unprocessedIDS.Add(rguid);
                    }
                }
                foreach(var unprocessed in unprocessedIDS)
                {
                    unprocessed.id = startID;
                    startID++;
                }
            }
        }
    }
}
