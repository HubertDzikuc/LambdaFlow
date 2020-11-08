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

    [Serializable]
    public class TransformPayload : Payload
    {
        public float X;
        public float Y;
        public float Z;

        [JsonIgnore]
        public Vector3 Position => new Vector3(X, Y, Z);

        public TransformPayload(Vector3 position)
        {
            this.X = position.x;
            this.Y = position.y;
            this.Z = position.z;
        }
    }
}
