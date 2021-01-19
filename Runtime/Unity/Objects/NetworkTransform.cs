using Multiplayer.API.Lambda;
using System;
using UnityEngine;

namespace Multiplayer.API.Unity.Lambda
{
    public class NetworkTransform
    {
        private readonly NetworkCommand<Action<TransformPayload>> updateTransformAction;

        private readonly Transform transform;

        public NetworkTransform(Transform transform)
        {
            this.transform = transform;

            updateTransformAction = new NetworkCommand<Action<TransformPayload>>(UpdateTransformData);

            updateTransformAction.RunOnUpdate(new TransformPayload(this.transform));
        }

        private void UpdateTransformData(TransformPayload payload)
        {
            Debug.Log(payload);
            transform.position = payload.Position;
            transform.rotation = payload.Rotation;
        }
    }
}