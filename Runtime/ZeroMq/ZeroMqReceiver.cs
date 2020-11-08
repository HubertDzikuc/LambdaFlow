using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroMQ;

namespace Laparo.Sim.ZeroMQ
{
    /// <summary>
    /// Wrapper around ZeroMQ Subscriber socket
    /// </summary>
    public class ZeroMQSubscriber : ZeroMQHandler
    {
        public ZeroMQSubscriber(string endpoint, string topic, out bool success)
        {
            success = Initialize(endpoint, ZSocketType.SUB, socket =>
            {
                socket.Connect(endpoint);
                socket.Subscribe(topic);
            });
        }

        public void OnReceive(Action<string> subscriber) => InternalOnReceive(subscriber);
    }

}
