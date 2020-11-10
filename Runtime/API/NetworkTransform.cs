using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    public class NetworkTransform<M> : NetworkObject<M> where M : MonoBehaviour
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

        private Transform transform;

        public NetworkTransform(Transform transform, M parent) : base(parent)
        {
            this.transform = transform;

            Register<TransformPayload>(NetworkMode.Server, UpdateTransformData);
        }

        public override void Update()
        {
            // The if is not needed but helps the performance because we are not 
            // invoking the method localy on client which would do nothing
            if (NetworkHandler.CurrentMode == NetworkMode.Server)
            {
                Invoke(UpdateTransformData, new TransformPayload(transform));
            }
        }

        private void UpdateTransformData(TransformPayload payload)
        {
            if (NetworkHandler.CurrentMode == NetworkMode.Client)
            {
                transform.position = payload.Position;
                transform.rotation = payload.Rotation;
            }
        }

    }
}