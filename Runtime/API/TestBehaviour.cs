using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aggressors.API
{
    public class TestBehaviour : NetworkBehaviour<TestBehaviour>
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

        //private NetworkObject<TestBehaviour> networkObject = new NetworkObject<TestBehaviour>();

        private int i = 0;

        public void SendTest(string msg) => Invoke(InternalSendTest, msg);
        public void SendTestPayload(TestPayload payload) => Invoke(InternalSendTestPayload, payload);

        private TestPayload InternalSendTest(string testText)
        {
            Debug.Log($"{nameof(InternalSendTest)} {testText}");
            return new TestPayload(testText);
        }

        private TestPayload InternalSendTestPayload(TestPayload payload)
        {
            Debug.Log($"{nameof(InternalSendTestPayload)} {payload.text}");
            return payload;
        }

        protected override void Register()
        {
            Debug.Log("Register");
            //can send only the same as current mode and recieve the different 
            Register<string, TestPayload>(NetworkMode.Client, InternalSendTest, pay => pay.text);
            Register<TestPayload>(NetworkMode.Server, InternalSendTestPayload);
        }

        private void Update()
        {
            if (i < 5)
            {
                Debug.Log($"Invoke {i}");
                SendTest($"Test {i}");
                SendTestPayload(new TestPayload($"TestPayload {i}"));
                i++;
            }
        }
    }
}