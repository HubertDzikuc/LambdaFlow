using Multiplayer.API.System;
using System;

namespace Multiplayer.API.Lambda
{
    public abstract class ILambda<T>
    {
        public NetworkMode Mode { get; private set; }
        protected Action<T> Action { get; private set; }

        protected ILambda(NetworkMode mode, Action<T> action)
        {
            this.Mode = mode;
            this.Action = action;
        }

        public virtual void Invoke(T argument)
        {
            if (NetworkHandler.Mode == Mode)
            {
                Action?.Invoke(argument);
            }
        }

        public virtual void RunInUpdate(Func<T> argument, UpdateTiming updateTiming = UpdateTiming.OncePerUpdate)
        {
            NetworkHandler.RunInUpdate(Mode, () => Invoke(argument()), updateTiming);
        }
    }
}