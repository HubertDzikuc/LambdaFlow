using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class LocalClientLambda<T> : ILambda<T>
    {
        public LocalClientLambda(Action<T> action) : base(NetworkMode.Client, action) { }
    }
}