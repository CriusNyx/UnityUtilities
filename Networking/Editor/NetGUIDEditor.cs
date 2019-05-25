using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityUtilities.Networking;

[CustomEditor(typeof(GUIDPool))]
public class NetGUIDEditor : Editor
{
    string[] arr;

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Get Map"))
        {
            arr = GUIDPool.GetMappedIDs().Select
                (
                x => x.Key + ":" + x.Value?.behaviour?.name
                ).ToArray();
        }

        if(arr != null)
        {
            foreach(var line in arr)
            {
                GUILayout.Label("\t" + line);
            }
        }
    }
}