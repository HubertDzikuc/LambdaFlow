using Multiplayer.API.Lambda;
using UnityEngine;

namespace Multiplayer.API.Unity.Lambda
{
    public class NetworkTransform
    {
        private readonly NetworkCommand<TransformPayload> updateTransformAction;

        private readonly Transform transform;

        public NetworkTransform(Transform transform)
        {
            this.transform = transform;

            updateTransformAction = new NetworkCommand<TransformPayload>(UpdateTransformData);

            updateTransformAction.RunInUpdate(() => new TransformPayload(this.transform));
        }

        private void UpdateTransformData(TransformPayload payload)
        {
            transform.position = payload.Position;
            transform.rotation = payload.Rotation;
        }
    }
}