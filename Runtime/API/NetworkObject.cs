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
            NetworkId = NetworkHandler.RegisterNetworkObject<M>().ToString();
        }

        public static string ClassTag => typeof(M).Name;

        public string NetworkId { get; private set; }

        public void Invoke<T>(Action<T> action, T argument) where T : Payload => NetworkHandler.Invoke(ClassTag, NetworkId, action, argument);

        public void Register<T>(NetworkMode mode, Action<T> action) where T : Payload
        {
            NetworkHandler.Register(mode, ClassTag, NetworkId, action.Method.Name, p =>
             {
                 if (p.ToString().TryParseJson<T>(out var payload))
                 {
                     action(payload);
                 }
             });
        }

        public void Dispose()
        {
            NetworkHandler.Deregister(ClassTag, NetworkId);
        }
    }
}
