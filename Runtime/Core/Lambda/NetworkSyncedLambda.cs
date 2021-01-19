using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class NetworkSyncedLambda<T> : NetworkLambda<T> where T : Delegate
    {
        public NetworkSyncedLambda(T action) : base(NetworkMode.Server, action) { }

        protected override bool Active => NetworkHandler.Mode == NetworkMode.Client;

        protected override void LocalInvoke(params object[] arguments)
        {
            if (Active)
            {
                InvokeAction(arguments);
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