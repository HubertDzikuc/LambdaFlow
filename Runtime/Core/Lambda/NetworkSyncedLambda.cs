using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public class NetworkSyncedLambda<T1> : AbstractNetworkSyncedLambda<LambdaPayload<T1>>
    {
        public NetworkSyncedLambda(Action<T1> action) : base(action) { }

        public void Invoke(T1 argument1) => InternalInvoke(new LambdaPayload<T1>(argument1));
    }

    public class NetworkSyncedLambda<T1, T2> : AbstractNetworkSyncedLambda<LambdaPayload<T1, T2>>
    {
        public NetworkSyncedLambda(Action<T1, T2> action) : base(action) { }
        public void Invoke(T1 argument1, T2 argument2) => InternalInvoke(new LambdaPayload<T1, T2>(argument1, argument2));
    }

    public abstract class AbstractNetworkSyncedLambda<T> : AbstractNetworkLambda<T> where T : LambdaPayload
    {
        protected AbstractNetworkSyncedLambda(Delegate action) : base(NetworkMode.Server, action) { }

        protected override void InternalInvoke(T argument)
        {
            if (NetworkHandler.Mode == NetworkMode.Server)
            {
                InvokeAction(argument);
                Send(argument);
            }
        }

        protected override Reply AfterParse(T payload, out bool propagateArgument)
        {
            if (NetworkHandler.Mode == NetworkMode.Client)
            {
                InvokeAction(payload);
            }

            propagateArgument = false;

            return Reply.Success;
        }
    }
}