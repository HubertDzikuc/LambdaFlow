using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class LocalClientLambda<T> : Lambda<T> where T : Delegate
    {
        public LocalClientLambda(T action) : base(NetworkMode.Client, action) { }
    }

}