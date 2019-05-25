using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityUtilities.Networking;

[CustomEditor(typeof(NetworkServer))]
public class NetworkServerEditor : Editor
{
    (int clientNumber, float serverTime, float clientTime, float rtt, float offset)[] pingTimes;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NetworkServer server = target as NetworkServer;

        GUILayout.Label("IPs");
        foreach(var s in server.IPList)
        {
            GUILayout.Label("\t" + s);
        }

        GUILayout.Label("Clients");
        foreach(var client in server.GetClientList())
        {
            GUILayout.Label("\t" + client.ToString());
        }

        GUILayout.Label("Ping Times");
        if(GUILayout.Button("Get Ping Times"))
        {
            pingTimes = server.GetClientPingTimes().ToArray();
        }
        if(pingTimes != null)
        {
            foreach(var (clientNumber, serverTime, clientTime, rtt, offset) in pingTimes)
            {
                GUILayout.Label(string.Format("\tClientNumber: {0}, ServerTime: {1}, clientTime: {2} rtt: {3} offset: {4}", clientNumber, serverTime, clientTime, rtt, offset));
            }
        }

        GUILayout.Label("Incoming Events");
        foreach (string o in server.IncomingObjectList())
        {
            GUILayout.Label("\t" + o);
        }

        GUILayout.Label("Outgoing Events");
        foreach (string o in server.OutgoingObjectList())
        {
            GUILayout.Label("\t" + o);
        }

        Repaint();
    }
}