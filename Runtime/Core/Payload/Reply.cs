using System;

namespace Multiplayer.API.Payloads
{
    [Serializable]
    public class Reply : Payload
    {
        public ReplyStatus ReplyStatus { get; private set; }
        public string Message { get; private set; }

        public static Reply Success(string message = "") => new Reply(ReplyStatus.Success, message);
        public static Reply ParsingError(string message = "") => new Reply(ReplyStatus.ParsingError, message);
        public static Reply InvalidMode(string message = "") => new Reply(ReplyStatus.InvalidMode, message);
        public static Reply InvalidRequest(string message = "") => new Reply(ReplyStatus.InvalidRequest, message);
        public static Reply InternalError(string message = "") => new Reply(ReplyStatus.InternalError, message);

        public Reply(ReplyStatus status, string message)
        {
            this.ReplyStatus = status;
            this.Message = message;
        }
    }
}