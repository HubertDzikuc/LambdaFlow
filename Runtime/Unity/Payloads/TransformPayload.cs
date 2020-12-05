using Multiplayer.API.Lambda;
using Multiplayer.API.Payloads;
using System;
using UnityEngine;

namespace Multiplayer.API.Unity
{
    [Serializable]
    public class TransformPayload : Payload
    {
        public float X;
        public float Y;
        public float Z;

        public float Rx;
        public float Ry;
        public float Rz;
        public float Rw;

        public Vector3 Position => new Vector3(X, Y, Z);
        public Quaternion Rotation => new Quaternion(Rx, Ry, Rz, Rw);

        public TransformPayload() { }

        public TransformPayload(Transform transform)
        {
            this.X = transform.position.x;
            this.Y = transform.position.y;
            this.Z = transform.position.z;

            this.Rx = transform.rotation.x;
            this.Ry = transform.rotation.y;
            this.Rz = transform.rotation.z;
            this.Rw = transform.rotation.w;
        }
    }
}