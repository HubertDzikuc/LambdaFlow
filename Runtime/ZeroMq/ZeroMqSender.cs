using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroMQ;

namespace Laparo.Sim.ZeroMQ
{
    /// <summary>
    /// Wrapper around ZeroMQ Publisher socket
    /// </summary>
    public class ZeroMQPublisher : ZeroMQHandler
    {
        public ZeroMQPublisher(string endpoint, out bool success)
        {
            success = Initialize(endpoint, ZSocketType.PUB, socket => socket.Bind(endpoint));
        }

        public void Send(string msg) => InternalSend(msg);
    }

}
