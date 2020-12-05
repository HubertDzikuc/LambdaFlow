using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroMQ;

namespace Laparo.Sim.ZeroMQ
{
    /// <summary>
    /// Class responsible for easier ZeroMQ handling
    /// </summary>
    public abstract class ZeroMQHandler : IDisposable
    {
        protected ZSocket Socket { get; private set; }

        protected ZPollItem Poller { get; private set; }

        protected virtual bool Initialize(string endpoint, ZSocketType type, Action<ZSocket> modifySocketCallback)
        {
            // Disable ZMQ for non-windows platforms.
            if (Application.platform != RuntimePlatform.WindowsEditor
                && Application.platform != RuntimePlatform.WindowsPlayer)
            {
                Debug.LogWarning($"ZMQ Connection is not supported on this platform. {nameof(ZeroMQHandler)} will be disabled.");
                return false;
            }
            Poller = ZPollItem.CreateReceiver();
            Socket = new ZSocket(type);
            modifySocketCallback(Socket);
            return true;
        }

        protected void InternalSend(string msg)
        {
            Socket.Send(new ZFrame(msg));
        }

        protected void InternalOnReceive(Action<string> subscriber)
        {
            while (true)
            {
                if (Socket.PollIn(Poller, out var messages, out var error, TimeSpan.FromSeconds(0)))
                {
                    if ((Poller.ReadyEvents & ZPoll.In) != ZPoll.None)
                    {
                        string message = "";
                        for (int i = 0; i < messages.Count; i++)
                        {
                            var frame = messages[i];
                            message += frame.ReadString();
                        }
                        subscriber?.Invoke(message);
                    }

                }
                else if (error == ZError.ETERM)
                {
                    Debug.LogError("ETERM");
                    return;    // Interrupted
                }
                else if (error != ZError.EAGAIN)
                    throw new ZException(error);
                else
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            if (Socket != null)
            {
                Socket.Dispose();
            }
        }
    }

}
