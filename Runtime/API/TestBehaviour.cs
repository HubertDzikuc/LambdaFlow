using UnityEngine;

namespace Multiplayer.API
{
    public class TestBehaviour : NetworkMonoBehaviour<TestBehaviour>
    {
        protected override Rigidbody2D Rigidbody2D => GetComponent<Rigidbody2D>();

        public class TestPayload : Payload
        {
            public string text = "dabdafjaf";

            public TestPayload(string str)
            {
                Debug.Log($"New {nameof(TestPayload)} {str}");

                this.text = str;
            }
        }

        private int i = 0;

        public void SendTest(TestPayload payload) => Invoke(InternalSendPayload, payload);

        private void InternalSendPayload(TestPayload payload)
        {
            Debug.Log($"{nameof(InternalSendPayload)} {payload.text}");
        }

        protected override void Register()
        {
            Debug.Log("Register");
            Register<TestPayload>(NetworkMode.Server, InternalSendPayload);
        }

        protected override void Update()
        {
            base.Update();

            if (i < 5)
            {
                Debug.Log($"Invoke {i}");
                SendTest(new TestPayload($"TestPayload {i}"));
                i++;
            }
        }
    }
}