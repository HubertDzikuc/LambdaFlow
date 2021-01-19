using System;

namespace Multiplayer.API.Payloads
{
    [Serializable]
    public class Command : Payload
    {
        public string Id;
        public object[] Payload;

        public Command(string id, object[] payload)
        {
            Id = id;
            Payload = payload;
        }
    }
}