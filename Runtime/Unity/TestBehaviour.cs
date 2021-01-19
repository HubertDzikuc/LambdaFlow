using Multiplayer.API.Lambda;
using Multiplayer.API.Payloads;
using Multiplayer.API.Unity.Lambda;
using System;
using UnityEngine;

namespace Multiplayer.API.Unity.Tests
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

        public NetworkCommand<Action<string, int>> SendPayload;

        public NetworkRequest<Action<string>> SpawnItself;

        private void LocalSendPayload(string str, int count)
        {
            Debug.Log($"{nameof(LocalSendPayload)} {str} {count}");
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
            SendPayload = new NetworkCommand<Action<string, int>>(LocalSendPayload);
            SpawnItself = new NetworkRequest<Action<string>>(LocalSpawnItself);
            SpawnItself.Invoke("Spawn new class");
        }

        private int i = 0;
        private void Update()
        {
            SendPayload.Invoke($"Invoking {nameof(SendPayload)} {i}", 14214124);
        }
    }
}