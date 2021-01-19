using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class NetworkRequest<T> : NetworkLambda<T> where T : Delegate
    {
        public NetworkRequest(T action) : base(NetworkMode.Client, action) { }

        protected override bool Active => true;

        protected override void LocalInvoke(params object[] arguments)
        {
            if (NetworkHandler.IsMode(NetworkMode.Host, NetworkMode.SinglePlayer))
            {
                InvokeAction(arguments);
            }

            if (NetworkHandler.Mode == NetworkMode.Client)
            {
                Send(arguments);
            }
        }

        protected override Reply AfterParse(out bool propagateArgument, params object[] arguments)
        {
            InvokeAction(arguments);

            propagateArgument = false;
            if (NetworkHandler.Mode == NetworkMode.Server)
            {
                propagateArgument = true;
            }

            return Reply.Success();
        }
    }
}