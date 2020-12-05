using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class NetworkRequest<T> : INetworkLambda<T>
    {
        public NetworkRequest(Action<T> action) : base(NetworkMode.Client, action) { }
        public override void Invoke(T argument)
        {
            if (NetworkHandler.Mode == NetworkMode.Client)
            {
                Sender(Id, argument);
            }
        }

        protected override Reply AfterParse(T payload, object argument)
        {
            Action(payload);

            if (NetworkHandler.Mode == NetworkMode.Server)
            {
                Sender(Id, argument);
            }

            return Reply.Success;
        }
    }
}