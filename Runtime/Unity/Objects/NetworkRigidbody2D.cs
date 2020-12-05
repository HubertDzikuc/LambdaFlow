using Multiplayer.API.Lambda;
using UnityEngine;

namespace Multiplayer.API.Unity.Lambda
{
    public class NetworkRigidbody2D
    {
        private readonly NetworkCommandPayload<Rigidbody2DPayload> updateRigidbodyAction;

        private readonly Rigidbody2D rigidbody;

        public NetworkRigidbody2D(Rigidbody2D rigidbody)
        {
            this.rigidbody = rigidbody;

            updateRigidbodyAction = new NetworkCommandPayload<Rigidbody2DPayload>(UpdateRigidbodyData);

            updateRigidbodyAction.RunInUpdate(() => new Rigidbody2DPayload(this.rigidbody));
        }

        private void UpdateRigidbodyData(Rigidbody2DPayload payload)
        {
            rigidbody.angularVelocity = payload.AngularVelocity;
            rigidbody.velocity = payload.Velocity;
        }
    }
}