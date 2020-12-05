using System;
using System.Collections.Generic;

namespace Multiplayer.API.Payloads
{
    [Serializable]
    public abstract class BasePayload
    {
        public abstract IEnumerable<object> GetArguments();
    }

    [Serializable]
    public abstract class Payload : BasePayload
    {
        public override IEnumerable<object> GetArguments()
        {
            yield return this;
        }
    }

}
