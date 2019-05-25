using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityUtilities.Networking;

public class TestBehaviourSpawner : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            var networkGameObject = new GameObject("Network");
            var server = networkGameObject.AddComponent<NetworkServer>();
            var client = NetworkClient.Create(networkGameObject, "127.0.0.1");
        }

        if(Input.GetKeyDown(KeyCode.I))
        {
            var networkGameObject = new GameObject("Network");
            var client = NetworkClient.Create(networkGameObject, "127.0.0.1");
        }

        if(Input.GetKeyDown(KeyCode.K))
        {
            GameObject.CreatePrimitive(PrimitiveType.Sphere).AddNetComponent<SpawnTestBehaviour>();
        }
    }
}