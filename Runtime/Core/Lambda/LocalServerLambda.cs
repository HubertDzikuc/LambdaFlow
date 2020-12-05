using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class LocalServerLambda<T> : Lambda<T>
    {
        public LocalServerLambda(Action<T> action) : base(NetworkMode.Server, action) { }
    }
}