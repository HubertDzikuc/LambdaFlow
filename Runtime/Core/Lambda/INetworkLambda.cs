using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using Multiplayer.API.Utils;
using System;

namespace Multiplayer.API.Lambda
{
    public abstract class INetworkLambda<T> : ILambda<T>, IDisposable
    {
        public int Id { get; private set; }
        protected Func<int, object, bool> Sender { get; private set; }

        private Action disposer;

        protected INetworkLambda(NetworkMode mode, Action<T> action) : base(mode, action)
        {
            NetworkHandler.Instance.Register(Receive, out int id, out var sender, out disposer);
            Id = id;
            Sender = sender;
        }

        protected virtual Reply AfterParse(T payload, object argument)
        {
            Action(payload);

            return Reply.Success;
        }

        private Reply Receive(object argument)
        {
            if (argument.ToString().TryParseJson<T>(out var payload))
            {
                return ReplyInfo(AfterParse(payload, argument), argument);
            }
            else
            {
                return ReplyInfo(Reply.ParsingError, argument);
            }
        }

        private Reply ReplyInfo(Reply reply, object argument)
        {
            switch (reply.ReplyStatus)
            {
                case ReplyStatus.ParsingError:
                    NetworkHandler.Log.LogWarning($"Couldn't parse data {argument} to type {typeof(T)} ({Mode} {Action.Method.Name})");
                    return Reply.ParsingError;
                default:
                    return reply;
            }
        }

        public void Dispose() => disposer();
    }

}