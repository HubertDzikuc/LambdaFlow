using System;

namespace Multiplayer.API.Payloads
{
    [Serializable]
    public class Command : Payload
    {
        public string Id;
        public string Payload;

        public Command(string id, string payload)
        {
            Id = id;
            Payload = payload;
        }
    }
}