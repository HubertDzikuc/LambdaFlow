using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class NetworkSyncedLambda<T> : INetworkLambda<T>
    {
        public NetworkSyncedLambda(Action<T> action) : base(NetworkMode.Server, action) { }
        public override void Invoke(T argument)
        {
            if (NetworkHandler.Mode == NetworkMode.Server)
            {
                Action(argument);
                Sender(Id, argument);
            }
        }

        protected override Reply AfterParse(T payload, object argument)
        {
            if (NetworkHandler.Mode == NetworkMode.Client)
            {
                Action(payload);
            }

            return Reply.Success;
        }
    }
}