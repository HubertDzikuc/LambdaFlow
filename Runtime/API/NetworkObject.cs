using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    public class NetworkObject<M> : IDisposable where M : MonoBehaviour
    {
        public NetworkObject()
        {
            NetworkId = NetworkHandler.RegisterNetworkObject(this).ToString();
        }

        public static string ClassTag => typeof(M).Name;

        public string NetworkId { get; private set; } = "0";

        public void Invoke<T, P>(Func<T, P> func, T argument) where P : Payload => NetworkHandler.Send(ClassTag, NetworkId, func.Method.Name, func(argument));

        public void Register<T>(NetworkMode mode, Func<T, T> func) where T : Payload => Register(mode, func, p => p);

        public void Register<T, P>(NetworkMode mode, Func<T, P> func, Func<P, T> toArgument) where P : Payload
        {
            NetworkHandler.Register(mode, ClassTag, NetworkId, func.Method.Name, p =>
             {
                 if (FromJson<P>(p, out var payload))
                 {
                     func(toArgument(payload));
                 }
             });
        }

        protected bool FromJson<T>(object json, out T payload) where T : Payload => json.ToString().TryParseJson(out payload);

        public void Dispose()
        {
            NetworkHandler.Deregister(ClassTag, NetworkId);
        }
    }
}
