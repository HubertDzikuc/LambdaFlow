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

        private NetworkTransform networkTransform;
        private NetworkRigidbody2D networkRigidbody;

        public NetworkSyncedTask<string> SendPayload;

        private void LocalSendPayload(string str)
        {
            Debug.Log($"{nameof(LocalSendPayload)} {str}");
        }

        private void Start()
        {
            networkTransform = new NetworkTransform(transform);
            networkRigidbody = new NetworkRigidbody2D(GetComponent<Rigidbody2D>());
            SendPayload = new NetworkSyncedTask<string>(LocalSendPayload);
        }

        private int i = 0;
        private void Update()
        {
            SendPayload.Invoke($"Invoking {nameof(SendPayload)} {i}");
        }
    }
}