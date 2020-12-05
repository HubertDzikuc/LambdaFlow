using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class LocalClientLambda<T1> : Lambda<T1>
    {
        public LocalClientLambda(Action<T1> action) : base(NetworkMode.Client, action) { }
    }

    public class LocalClientLambda<T1, T2> : Lambda<T1, T2>
    {
        public LocalClientLambda(Action<T1, T2> action) : base(NetworkMode.Client, action) { }
    }
}