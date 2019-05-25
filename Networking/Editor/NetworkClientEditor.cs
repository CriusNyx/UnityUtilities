using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityUtilities.Networking;

[CustomEditor(typeof(NetworkClient))]
public class NetworkClientEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NetworkClient client = target as NetworkClient;

        GUILayout.Label("Events");
        foreach(var o in client.ObjectList())
        {
            GUILayout.Label("\t" + o);
        }

        Repaint();
    }
}