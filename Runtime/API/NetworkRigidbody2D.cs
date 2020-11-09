using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    public class NetworkRigidbody2D<M> : NetworkObject<M> where M : MonoBehaviour
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

        private Rigidbody2D rigidbody;

        public NetworkRigidbody2D(Rigidbody2D rigidbody) : base()
        {
            this.rigidbody = rigidbody;

            Register<Rigidbody2DPayload>(NetworkMode.Server, UpdateRigidbodyData);
        }

        public void Update()
        {
            // The if is not needed but helps the performance because we are not 
            // invoking the method localy on client which would do nothing
            if (NetworkHandler.CurrentMode == NetworkMode.Server)
            {
                Invoke(UpdateRigidbodyData, new Rigidbody2DPayload(rigidbody));
            }
        }

        private void UpdateRigidbodyData(Rigidbody2DPayload payload)
        {
            if (NetworkHandler.CurrentMode == NetworkMode.Client)
            {
                rigidbody.angularVelocity = payload.AngularVelocity;
                rigidbody.velocity = payload.Velocity;
            }
        }
    }
}