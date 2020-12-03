using System;
using UnityEngine;

namespace Multiplayer.API
{
    [Serializable]
    public abstract class Payload { }

    public enum ReplyStatus
    {
        Success,
        ParsingError,
        InvalidMode,
        InvalidRequest,
        InternalError,
    }

    [Serializable]
    public class Reply : Payload
    {
        public ReplyStatus ReplyStatus { get; private set; }

        public static Reply Success => new Reply(ReplyStatus.Success);
        public static Reply ParsingError => new Reply(ReplyStatus.ParsingError);
        public static Reply InvalidMode => new Reply(ReplyStatus.InvalidMode);
        public static Reply InvalidRequest => new Reply(ReplyStatus.InvalidRequest);
        public static Reply InternalError => new Reply(ReplyStatus.InternalError);

        public Reply(ReplyStatus status)
        {
            this.ReplyStatus = status;
        }
    }
}
