using System;

namespace Multiplayer.API.System
{
    public interface ILog
    {
        void LogError(Exception ex);
        void LogError(string msg);
        void LogWarning(string msg);
    }
}