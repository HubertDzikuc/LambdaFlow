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

        private NetworkObject<TestBehaviour> networkBehaviour;

        public Action<string> SendPayload;

        private void LocalSendPayload(string str)
        {
            Debug.Log($"{nameof(LocalSendPayload)} {str}");
        }

        private void Start()
        {
            networkBehaviour = new NetworkObject<TestBehaviour>(this)
                .Register(NetworkMode.Server, LocalSendPayload, out SendPayload)
                .Register(() => Value);
        }

        private int i = 0;
        private void Update()
        {
            //if (i < 5)
            {
                // Debug.Log($"Invoke {i}");
                // SendPayload($"Invoking {nameof(SendPayload)} {i}");
                if (NetworkHandler.CurrentMode == NetworkMode.Server)
                {
                    Value = $"ValueChangedByServer {i}";
                    networkBehaviour.Update();
                    // Debug.Log(Value);
                }
                else
                {
                    Value = $"ValueChangedByClient {i}";
                    networkBehaviour.Update();
                    // Debug.Log(Value);
                }
                i++;
            }
            // Debug.Log(Value);
        }
    }
}