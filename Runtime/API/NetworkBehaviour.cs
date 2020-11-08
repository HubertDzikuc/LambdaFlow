using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    public abstract class NetworkBehaviour<M> : MonoBehaviour, IDisposable where M : MonoBehaviour
    {
        private NetworkObject<M> networkObject;
        private NetworkTransform<M> networkTransform;

        protected void Register<T>(NetworkMode mode, Func<T, T> func) where T : Payload => networkObject.Register(mode, func);
        protected void Register<T, P>(NetworkMode mode, Func<T, P> func, Func<P, T> toArgument) where P : Payload => networkObject.Register(mode, func, toArgument);
        protected void Invoke<T, P>(Func<T, P> func, T argument) where P : Payload => networkObject.Invoke(func, argument);

        protected abstract void Register();

        protected virtual void Start()
        {
            networkObject = new NetworkObject<M>();
            networkTransform = new NetworkTransform<M>(transform);
            Register();
        }

        protected virtual void Update()
        {
            networkTransform.Update();
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            networkObject.Dispose();
        }
    }
}
