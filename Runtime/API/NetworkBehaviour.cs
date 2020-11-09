using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    public class NetworkBehaviour<M> : IDisposable where M : MonoBehaviour
    {
        private NetworkObject<M> networkObject;
        private NetworkTransform<M> networkTransform;
        private NetworkRigidbody2D<M> networkRigidbody2D;

        public void Register<T>(NetworkMode mode, Action<T> action) where T : Payload => networkObject.Register(mode, action);
        public void Invoke<T>(Action<T> action, T argument) where T : Payload => networkObject.Invoke(action, argument);

        /// <summary>
        /// Remember to invoke it only during/after Start
        /// </summary>
        public NetworkBehaviour()
        {
            networkObject = new NetworkObject<M>();
        }

        /// <summary>
        /// Remember to invoke it only during/after Start
        /// </summary>
        public NetworkBehaviour(Transform transform)
        {
            networkObject = new NetworkObject<M>();

            if (transform != null)
            {
                networkTransform = new NetworkTransform<M>(transform);
            }
        }

        /// <summary>
        /// Remember to invoke it only during/after Start
        /// </summary>
        public NetworkBehaviour(Transform transform, Rigidbody2D rigidbody)
        {
            networkObject = new NetworkObject<M>();

            if (transform != null)
            {
                networkTransform = new NetworkTransform<M>(transform);
            }
            if (rigidbody != null)
            {
                networkRigidbody2D = new NetworkRigidbody2D<M>(rigidbody);
            }
        }

        public void Update()
        {
            networkTransform?.Update();
            networkRigidbody2D?.Update();
        }

        public void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            networkObject.Dispose();
            networkTransform.Dispose();
            networkRigidbody2D.Dispose();
        }
    }
}
