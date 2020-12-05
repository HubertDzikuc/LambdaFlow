using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using Multiplayer.API.Utils;
using System;

namespace Multiplayer.API.Lambda
{
    public abstract class AbstractNetworkLambda<T> : AbstractLambda<T>, IDisposable where T : BasePayload
    {
        private readonly int Id;
        private readonly Func<int, object, bool> Sender;

        private readonly Action Disposer;

        protected AbstractNetworkLambda(NetworkMode mode, Delegate action) : base(mode, action)
        {
            NetworkHandler.Instance.Register(Receive, out Id, out Sender, out Disposer);
        }

        protected virtual Reply AfterParse(T payload, out bool propagateArgument)
        {
            InvokeAction(payload);

            propagateArgument = false;

            return Reply.Success;
        }

        protected void Send(object payload)
        {
            if (NetworkHandler.IsMode(NetworkMode.SinglePlayer) == false)
            {
                Sender(Id, payload);
            }
        }

        private Reply Receive(object payload)
        {
            if (payload.ToString().TryParseJson<T>(out var argument))
            {
                var reply = AfterParse(argument, out bool propagateArgument);

                if (propagateArgument)
                {
                    Send(payload);
                }

                return ReplyInfo(reply, payload);
            }
            else
            {
                return ReplyInfo(Reply.ParsingError, payload);
            }
        }

        private Reply ReplyInfo(Reply reply, object argument)
        {
            switch (reply.ReplyStatus)
            {
                case ReplyStatus.ParsingError:
                    NetworkHandler.Log.LogWarning($"Couldn't parse data {argument} ({Mode} {ActionName})");
                    return Reply.ParsingError;
                default:
                    return reply;
            }
        }

        public void Dispose() => Disposer();
    }

}