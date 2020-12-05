using System;

namespace Multiplayer.API.System
{

    public interface ICommandsHandler
    {
        event Action UpdateEvent;
        event Action<string> ReceiveEvent;
        ILog Logger { get; }
        NetworkMode Mode { get; }
        void Send(string command);
    }
}