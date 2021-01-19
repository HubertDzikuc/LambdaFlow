using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class NetworkCommand<T> : NetworkLambda<T> where T : Delegate
    {
        public NetworkCommand(T action) : base(NetworkMode.Server, action) { }

        protected override bool Active => NetworkHandler.IsMode(NetworkMode.Client);

        protected override void LocalInvoke(params object[] arguments)
        {
            if (NetworkHandler.IsMode(NetworkMode.Host, NetworkMode.SinglePlayer))
            {
                InvokeAction(arguments);
            }

            if (NetworkHandler.Mode == NetworkMode.Server)
            {
                Send(arguments);
            }
        }

        protected override Reply AfterParse(out bool propagateArgument, params object[] arguments)
        {
            InvokeAction(arguments);

            propagateArgument = false;

            return Reply.Success();
        }
    }
}