using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityUtilities.Networking;

public class UDPTest : MonoBehaviour
{
    Thread thread;

    private void Start()
    {
        thread = new Thread(Run);
        thread.Start();
    }

    private void Run()
    {
        UdpClient server, client, serverToClient;
        server = new UdpClient(NetworkServer.UDP_PORT);
        client = new UdpClient();
        client.Connect("127.0.0.1", NetworkServer.UDP_PORT);

        Debug.Log("Client Connected");

        byte[] arr = new byte[] { 0, 1, 2, 3 };

        Debug.Log("Client Send");

        client.Send(arr, arr.Length);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, NetworkServer.UDP_PORT);

        Debug.Log("Server Receive");
        byte[] arr2 = server.Receive(ref endPoint);


        Debug.Log("Client Return");
        serverToClient = new UdpClient();
        //serverToClient.Connect();
        serverToClient.Send(arr2, arr2.Length, endPoint);

        Debug.Log("Client Receive");

        byte[] arr3 = client.Receive(ref endPoint);

        Debug.Log(ByteArrayToString(arr3));
    }

    private void OnDestroy()
    {
        thread.Abort();
    }

    public static string ByteArrayToString(byte[] ba)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }
}