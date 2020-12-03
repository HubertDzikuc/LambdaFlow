using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    public class NetworkRigidbody2D
    {
        [Serializable]
        public class Rigidbody2DPayload : Payload
        {
            public float AV;
            public float VX;
            public float VY;

            public Vector2 Velocity => new Vector2(VX, VY);
            public float AngularVelocity => AV;

            public Rigidbody2DPayload() { }

            public Rigidbody2DPayload(Rigidbody2D rigidbody)
            {
                AV = rigidbody.angularVelocity;
                VX = rigidbody.velocity.x;
                VY = rigidbody.velocity.y;
            }
        }

        private readonly NetworkCommand<Rigidbody2DPayload> updateRigidbodyAction;

        private readonly Rigidbody2D rigidbody;

        public NetworkRigidbody2D(Rigidbody2D rigidbody)
        {
            this.rigidbody = rigidbody;

            updateRigidbodyAction = new NetworkCommand<Rigidbody2DPayload>(UpdateRigidbodyData);

            updateRigidbodyAction.RunInUpdate(() => new Rigidbody2DPayload(this.rigidbody));
        }

        private void UpdateRigidbodyData(Rigidbody2DPayload payload)
        {
            rigidbody.angularVelocity = payload.AngularVelocity;
            rigidbody.velocity = payload.Velocity;
        }
    }
}