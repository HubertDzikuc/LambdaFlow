using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class NetworkCommand<T> : INetworkLambda<T>
    {
        public NetworkCommand(Action<T> action) : base(NetworkMode.Server, action) { }

        public override void Invoke(T argument)
        {
            if (NetworkHandler.Mode == NetworkMode.Server)
            {
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