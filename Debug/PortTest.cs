using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PortTest : MonoBehaviour
{
    Thread[] threads1;
    Thread[] threads2;

    private void Start()
    {
        int l = 1;
        threads1 = new Thread[l];
        threads2 = new Thread[l];
        for(int i = 0; i < l; i++)
        {
            Thread thread2 = new Thread(OffThread2Async);
            threads2[i] = thread2;
            thread2.Start();

            Thread thread = new Thread(OffThread1);
            threads1[i] = thread;
            thread.Start();
        }
    }

    private void OnDestroy()
    {
        foreach(var thread in threads1)
        {
            thread?.Abort();
        }
    }

    private void OffThread1()
    {
        //byte[] outBytes = Encoding.UTF7.GetBytes("Blob");
        byte[] outBytes = new byte[4];
        outBytes[0] = (byte)'B';
        outBytes[1] = (byte)'l';
        outBytes[2] = (byte)'o';
        outBytes[3] = (byte)'b';

        Debug.Log("Making Client");
        UdpClient client = new UdpClient();
        //IPAddress address = Dns.GetHostAddresses("demos.kaazing.com/echo")[0];
        client.Connect("8.8.8.8", 7);

        Debug.Log("Sending Bytes");
        client.Send(outBytes, outBytes.Length);
        Debug.Log("Bytes Sent");

        //UdpClient inClient = new UdpClient(client);

        //IPEndPoint endPoint = null;

        Debug.Log("Receiving Bytes");
        byte[] inBytes = client.ReceiveAsync().Result.Buffer;
        Debug.Log("Bytes Received");

        Debug.Log(inBytes);
    }

    private void OffThread2Async()
    {
        
    }
}