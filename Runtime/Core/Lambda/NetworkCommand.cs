using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class NetworkCommandPayload<T> : AbstractNetworkCommand<T> where T : BasePayload
    {
        public NetworkCommandPayload(Action<T> action) : base(action) { }

        public void Invoke(T payload) => InternalInvoke(payload);
    }

    public class NetworkCommand<T1> : AbstractNetworkCommand<LambdaPayload<T1>>
    {
        public NetworkCommand(Action<T1> action) : base(action) { }

        public void Invoke(T1 argument1) => InternalInvoke(new LambdaPayload<T1>(argument1));
    }

    public class NetworkCommand<T1, T2> : AbstractNetworkCommand<LambdaPayload<T1, T2>>
    {
        public NetworkCommand(Action<T1, T2> action) : base(action) { }
        public void Invoke(T1 argument1, T2 argument2) => InternalInvoke(new LambdaPayload<T1, T2>(argument1, argument2));
    }

    public abstract class AbstractNetworkCommand<T> : AbstractNetworkLambda<T> where T : BasePayload
    {
        protected AbstractNetworkCommand(Delegate action) : base(NetworkMode.Server, action) { }

        protected override void InternalInvoke(T argument)
        {
            if (NetworkHandler.IsMode(NetworkMode.Host, NetworkMode.SinglePlayer))
            {
                InvokeAction(argument);
            }

            if (NetworkHandler.Mode == NetworkMode.Server)
            {
                Send(argument);
            }
        }

        protected override Reply AfterParse(T argument, out bool propagateArgument)
        {
            if (NetworkHandler.Mode == NetworkMode.Client)
            {
                InvokeAction(argument);
            }

            propagateArgument = false;

            return Reply.Success;
        }
    }
}