using Multiplayer.API.Payloads;
using Multiplayer.API.System;
using Multiplayer.API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace Multiplayer.API.Lambda
{
    public abstract class NetworkLambda<T> : Lambda<T>, IDisposable where T : Delegate
    {
        private readonly int Id;
        private readonly Func<int, object[], bool> Sender;
        private readonly Action Disposer;
        protected virtual bool Active => NetworkHandler.IsMode(Mode);

        private Type[] agrumentTypes;

        protected NetworkLambda(NetworkMode mode, T action) : base(mode, action)
        {
            NetworkHandler.Instance.Register(Receive, out Id, out Sender, out Disposer);
            agrumentTypes = ExpressionsUtils.ParameterTypesOfDelegate<T>().ToArray();
        }

        protected virtual Reply AfterParse(out bool propagateArgument, params object[] arguments)
        {
            InvokeAction(arguments);

            propagateArgument = false;

            return Reply.Success();
        }

        protected void Send(params object[] payload)
        {
            if (NetworkHandler.IsMode(NetworkMode.SinglePlayer) == false)
            {
                Sender(Id, payload);
            }
        }

        private Reply Receive(object[] payload)
        {
            if (Active)
            {
                List<object> initialArguments = new List<object>();
                for (int i = 0; i < agrumentTypes.Length; i++)
                {
                    if (payload[i].ToString().TryParseJson(agrumentTypes[i], out var argument))
                    {
                        initialArguments.Add(argument);
                    }
                    else
                    {
                        return ParrsingError(Reply.ParsingError(), agrumentTypes[i], payload[i].ToString());
                    }
                }

                object[] arguments = initialArguments.ToArray();
                var reply = AfterParse(out bool propagateArgument, arguments);

                if (propagateArgument)
                {
                    Send(arguments);
                }
                return reply;
            }
            else
            {
                return Reply.Success();
            }
        }

        private Reply ParrsingError(Reply reply, Type type, object payload)
        {
            switch (reply.ReplyStatus)
            {
                case ReplyStatus.ParsingError:
                    string message = $"Couldn't parse data {payload} to type {type} ({Mode} {ActionName})";
                    NetworkHandler.Log.LogWarning(message);
                    return Reply.ParsingError(message);
                default:
                    return reply;
            }
        }

        public void Dispose() => Disposer();
    }

}