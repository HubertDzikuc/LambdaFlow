using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Multiplayer.API
{
    [Serializable]
    public abstract class Payload { }

    [Serializable]
    public class Reply : Payload
    {
        public string Status { get; private set; }

        public static Reply Success => new Reply("Success");
        public static Reply ParsingError => new Reply("ParsingError");
        public static Reply InvalidMode => new Reply("InvalidMode");
        public static Reply InvalidRequest => new Reply("InvalidRequest");
        public static Reply InternalError => new Reply("InternalError");

        public Reply(string status)
        {
            this.Status = status;
        }
    }
}
