using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class NetworkRequestCustom<T> : AbstractNetworkRequest<T> where T : BasePayload
    {
        public NetworkRequestCustom(Action<T> action) : base(action) { }

        public void Invoke(T payload) => InternalInvoke(payload);
    }

    public class NetworkRequest<T1> : AbstractNetworkRequest<LambdaPayload<T1>>
    {
        public NetworkRequest(Action<T1> action) : base(action) { }

        public void Invoke(T1 argument1) => InternalInvoke(new LambdaPayload<T1>(argument1));
    }

    public class NetworkRequest<T1, T2> : AbstractNetworkRequest<LambdaPayload<T1, T2>>
    {
        public NetworkRequest(Action<T1, T2> action) : base(action) { }
        public void Invoke(T1 argument1, T2 argument2) => InternalInvoke(new LambdaPayload<T1, T2>(argument1, argument2));
    }

    public abstract class AbstractNetworkRequest<T> : AbstractNetworkLambda<T> where T : BasePayload
    {
        protected AbstractNetworkRequest(Delegate action) : base(NetworkMode.Client, action) { }

        protected override void InternalInvoke(T argument)
        {
            if (NetworkHandler.IsMode(NetworkMode.Host, NetworkMode.SinglePlayer))
            {
                InvokeAction(argument);
            }

            if (NetworkHandler.Mode == NetworkMode.Client)
            {
                Send(argument);
            }
        }

        protected override Reply AfterParse(T argument, out bool propagateArgument)
        {
            InvokeAction(argument);

            propagateArgument = false;
            if (NetworkHandler.Mode == NetworkMode.Server)
            {
                propagateArgument = true;
            }

            return Reply.Success;
        }
    }
}