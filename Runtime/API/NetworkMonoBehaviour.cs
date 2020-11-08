using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    public abstract class NetworkMonoBehaviour<M> : MonoBehaviour, IDisposable where M : MonoBehaviour
    {
        private NetworkBehaviour<M> networkBehaviour;

        protected virtual Transform NetworkTransform => transform;
        protected virtual Rigidbody2D Rigidbody2D => GetComponent<Rigidbody2D>();

        protected void Register<T>(NetworkMode mode, Action<T> action) where T : Payload => networkBehaviour.Register(mode, action);
        protected void Invoke<T>(Action<T> action, T argument) where T : Payload => networkBehaviour.Invoke(action, argument);

        protected abstract void Register();

        protected virtual void Start()
        {
            networkBehaviour = new NetworkBehaviour<M>(NetworkTransform, Rigidbody2D);
            Register();
        }

        protected virtual void Update()
        {
            networkBehaviour.Update();
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            networkBehaviour.Dispose();
        }
    }
}
