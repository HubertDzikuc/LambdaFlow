using Multiplayer.API.Lambda;
using Multiplayer.API.Payloads;
using Newtonsoft.Json;
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

        [JsonIgnore]
        public Vector2 Velocity => new Vector2(VX, VY);
        [JsonIgnore]
        public float AngularVelocity => AV;

        public Rigidbody2DPayload() { }

        public Rigidbody2DPayload
            (Rigidbody2D rigidbody)
        {
            AV = rigidbody.angularVelocity;
            VX = rigidbody.velocity.x;
            VY = rigidbody.velocity.y;
        }
    }
}