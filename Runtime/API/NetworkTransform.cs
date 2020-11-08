using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    public class NetworkTransform<M> : NetworkObject<M> where M : MonoBehaviour
    {
        private Transform transform;

        public NetworkTransform(Transform transform) : base()
        {
            this.transform = transform;

            Register<TransformPayload>(NetworkMode.Server, SetPosition);
        }

        public void Update()
        {
            // The if is not needed but helps the performance because we are not 
            // invoking the method localy on client which would do nothing
            if (NetworkHandler.CurrentMode == NetworkMode.Server)
            {
                Invoke(SetPosition, new TransformPayload(transform.position));
            }
        }

        private TransformPayload SetPosition(TransformPayload payload)
        {
            
            if (NetworkHandler.CurrentMode == NetworkMode.Client)
            {
                transform.position = payload.Position;
            }
            return payload;
        }
    }
}