//using Open.Nat;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;
//using UnityUtilities.Networking;

//public class OpenPortTest : MonoBehaviour
//{
//    public string status = "";

//    private void Start()
//    {
//        Thread thread = new Thread(() => OpenPortAsync(NetworkServer.PORT_SERVER_IN));
//        thread.Start();
//    }

//    void OpenPortAsync(int portNum)
//    {
//        try
//        {
//            status = "Getting Network";
//            Debug.Log(status);

//            var discoverer = new NatDiscoverer();

//            status = "Getting Device";
//            Debug.Log(status);

//            var cts = new CancellationTokenSource(10000);
//            var device = discoverer.DiscoverDeviceAsync(PortMapper.Upnp | PortMapper.Pmp, cts).Result;

//            status = "Opening Port";
//            Debug.Log(status);

//            device.CreatePortMapAsync(new Mapping(Protocol.Udp, portNum, portNum));

//            status = "Done";
//            Debug.Log(status);
//        }
//        catch(Exception e)
//        {
//            Debug.LogError(e);
//            Debug.LogError(e.ToString());
//            status = e.Message;
//        }
//    }
//}