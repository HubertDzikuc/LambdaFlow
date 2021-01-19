using Multiplayer.API.System;
using Multiplayer.API.Utils;
using System;
namespace Multiplayer.API.Lambda
{
    public abstract class Lambda<T> where T : Delegate
    {
        protected string ActionName => action.Method.Name;

        public readonly NetworkMode Mode;
        public readonly T Invoke;

        protected readonly T action;

        public void RunOnUpdate(params object[] arguments)
        {
            var args = arguments;
            NetworkHandler.RunOnUpdate(Mode, () => LocalInvoke(args), UpdateTiming.OncePerUpdate);
        }

        protected Lambda(NetworkMode mode, T action)
        {
            this.Mode = mode;

            this.action = action;

            Invoke = ExpressionsUtils.GenerateDelegateFromInvoker<T>(this, LocalInvoke);
        }

        protected virtual void LocalInvoke(params object[] arguments)
        {
            if (NetworkHandler.IsMode(Mode, NetworkMode.Host, NetworkMode.SinglePlayer))
            {
                InvokeAction(arguments);
            }
        }

        protected void InvokeAction(params object[] arguments)
        {
            action?.DynamicInvoke(arguments);
        }
    }
}