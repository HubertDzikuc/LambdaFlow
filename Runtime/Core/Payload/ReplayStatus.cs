namespace Multiplayer.API.Payloads
{
    public enum ReplyStatus
    {
        Success = 200,
        ParsingError = 500,
        InvalidMode = 501,
        InvalidRequest = 502,
        InternalError = 300,
    }
}