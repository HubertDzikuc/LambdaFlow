using Multiplayer.API.Lambda;
using Multiplayer.API.Payloads;
using System;
using UnityEngine;

namespace Multiplayer.API.Unity
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
}