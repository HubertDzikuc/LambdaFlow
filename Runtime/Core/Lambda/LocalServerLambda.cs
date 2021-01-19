using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class LocalServerLambda<T> : Lambda<T> where T : Delegate
    {
        public LocalServerLambda(T action) : base(NetworkMode.Server, action) { }
    }
}