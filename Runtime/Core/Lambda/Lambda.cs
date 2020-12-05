using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Multiplayer.API.Lambda
{
    [Serializable]
    public abstract class LambdaPayload : BasePayload { }

    [Serializable]
    public class LambdaPayload<T1> : LambdaPayload
    {
        public T1 Argument1;

        public LambdaPayload(T1 argument1)
        {
            Argument1 = argument1;
        }

        public override IEnumerable<object> GetArguments()
        {
            yield return Argument1;
        }
    }

    [Serializable]
    public class LambdaPayload<T1, T2> : LambdaPayload
    {
        public T1 Argument1;
        public T2 Argument2;

        public LambdaPayload(T1 argument1, T2 argument2)
        {
            Argument1 = argument1;
            Argument2 = argument2;
        }

        public override IEnumerable<object> GetArguments()
        {
            yield return Argument1;
            yield return Argument2;
        }
    }

    public abstract class LambdaCustom<T> : AbstractLambda<T> where T : BasePayload
    {
        public LambdaCustom(NetworkMode mode, Action<T> action) : base(mode, action) { }

        public void Invoke(T payload) => InternalInvoke(payload);
    }

    public abstract class Lambda<T1> : AbstractLambda<LambdaPayload<T1>>
    {
        public Lambda(NetworkMode mode, Action<T1> action) : base(mode, action) { }

        public void Invoke(T1 argument1) => InternalInvoke(new LambdaPayload<T1>(argument1));
    }

    public abstract class Lambda<T1, T2> : AbstractLambda<LambdaPayload<T1, T2>>
    {
        public Lambda(NetworkMode mode, Action<T1, T2> action) : base(mode, action) { }

        public void Invoke(T1 argument1, T2 argument2) => InternalInvoke(new LambdaPayload<T1, T2>(argument1, argument2));
    }

    public abstract class AbstractLambda<T> where T : BasePayload
    {
        public NetworkMode Mode { get; private set; }
        private Delegate action;

        protected string ActionName => action.Method.Name;

        protected AbstractLambda(NetworkMode mode, Delegate action)
        {
            this.Mode = mode;
            this.action = action;
        }

        protected void InvokeAction(T payload)
        {
            action?.DynamicInvoke(payload.GetArguments().ToArray());
        }

        protected virtual void InternalInvoke(T payload)
        {
            if (NetworkHandler.IsMode(Mode, NetworkMode.Host, NetworkMode.SinglePlayer))
            {
                InvokeAction(payload);
            }
        }

        public virtual void RunInUpdate(Func<T> payload, UpdateTiming updateTiming = UpdateTiming.OncePerUpdate)
        {
            NetworkHandler.RunInUpdate(Mode, () => InternalInvoke(payload()), updateTiming);
        }
    }
}