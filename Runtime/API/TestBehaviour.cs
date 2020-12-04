using System;
using UnityEngine;

namespace Multiplayer.API
{
    public class TestBehaviour : MonoBehaviour
    {
        public class TestPayload : Payload
        {
            public string text = "dabdafjaf";

            public TestPayload(string str)
            {
                Debug.Log($"New {nameof(TestPayload)} {str}");

                this.text = str;
            }
        }

        public string Value = "ValueStart";

        private static int spawnedCount = 0;

        private NetworkTransform networkTransform;
        private NetworkRigidbody2D networkRigidbody;

        public NetworkSyncedTask<string> SendPayload;

        public NetworkRequest<string> SpawnItself;

        private void LocalSendPayload(string str)
        {
            Debug.Log($"{nameof(LocalSendPayload)} {str}");
        }

        private void LocalSpawnItself(string str)
        {
            Debug.Log($"{nameof(LocalSpawnItself)}");

            if (spawnedCount == 0)
            {
                Debug.Log($"{str}");
                Instantiate(this);
                spawnedCount++;
            }
        }

        private void Start()
        {
            networkTransform = new NetworkTransform(transform);
            networkRigidbody = new NetworkRigidbody2D(GetComponent<Rigidbody2D>());
            SendPayload = new NetworkSyncedTask<string>(LocalSendPayload);
            SpawnItself = new NetworkRequest<string>(LocalSpawnItself);

        }

        private int i = 0;
        private void Update()
        {
            SpawnItself.Invoke("Spawn new class");
            //  SendPayload.Invoke($"Invoking {nameof(SendPayload)} {i}");
        }
    }
}