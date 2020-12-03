using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    /// <summary>
    /// Invoke method only if the NetworkMode of the method is the same as
    /// Current NetworkMode and its Server, if its not server it sends the request to server for invoking the method
    /// which then resends its to every client and is then called
    /// </summary>
    public enum NetworkMode
    {
        Client,
        Server
    }

    public enum UpdateTiming
    {
        Never,
        OncePerUpdate
    }

    public interface ILog
    {
        void LogError(Exception ex);
        void LogError(string msg);
        void LogWarning(string msg);
    }

    public interface ICommandsHandler
    {
        Action UpdateEvent { get; set; }
        NetworkMode Mode { get; }
        void Send(string command);
    }

    public abstract class ITask<T>
    {
        public NetworkMode Mode { get; private set; }
        protected Action<T> Action { get; private set; }

        protected ITask(NetworkMode mode, Action<T> action)
        {
            this.Mode = mode;
            this.Action = action;
        }

        public virtual void Invoke(T argument)
        {
            if (NetworkHandler.Mode == Mode)
            {
                Action?.Invoke(argument);
            }
        }

        public virtual void RunInUpdate(Func<T> argument, UpdateTiming updateTiming = UpdateTiming.OncePerUpdate)
        {
            NetworkHandler.RunInUpdate(Mode, () => Invoke(argument()), updateTiming);
        }
    }

    public abstract class INetworkTask<T> : ITask<T>, IDisposable
    {
        public int Id { get; private set; }
        protected Func<int, object, bool> Sender { get; private set; }

        protected INetworkTask(NetworkMode mode, Action<T> action) : base(mode, action)
        {
            NetworkHandler.Instance.Register(Receive, out var id, out var sender);
            Id = id;
            Sender = sender;
        }

        public virtual Reply Receive(object argument)
        {
            if (NetworkHandler.Mode == NetworkMode.Client)
            {
                if (argument.ToString().TryParseJson<T>(out var payload))
                {
                    Action(payload);

                    if (NetworkHandler.Mode == NetworkMode.Server && Mode == NetworkMode.Client)
                    {
                        Sender(Id, argument);
                    }
                    return Reply.Success;
                }
                else
                {
                    NetworkHandler.Log.LogWarning($"Couldn't parse data {argument} to type {typeof(T)} ({Mode} {Action.Method.Name})");
                    return Reply.ParsingError;
                }
            }
            else
            {
                return Reply.InvalidMode;
            }
        }

        public void Dispose()
        {
            NetworkHandler.Instance.Deregister(this);
        }
    }

    public class LocalServerTask<T> : ITask<T>
    {
        public LocalServerTask(Action<T> action) : base(NetworkMode.Server, action) { }
    }

    public class LocalClientTask<T> : ITask<T>
    {
        public LocalClientTask(Action<T> action) : base(NetworkMode.Client, action) { }
    }

    public class NetworkRequest<T> : INetworkTask<T>
    {
        public NetworkRequest(Action<T> action) : base(NetworkMode.Client, action) { }
        public override void Invoke(T argument)
        {
            if (NetworkHandler.Mode == Mode)
            {
                Sender(Id, argument);
            }
        }
    }

    public class NetworkSyncedTask<T> : INetworkTask<T>
    {
        public NetworkSyncedTask(Action<T> action) : base(NetworkMode.Server, action) { }
        public override void Invoke(T argument)
        {
            if (NetworkHandler.Mode == Mode)
            {
                if (NetworkHandler.Mode == NetworkMode.Server)
                {
                    Action(argument);
                }
                Sender(Id, argument);
            }
        }
    }

    public class NetworkCommand<T> : INetworkTask<T>
    {
        public NetworkCommand(Action<T> action) : base(NetworkMode.Server, action) { }

        public override void Invoke(T argument)
        {
            if (NetworkHandler.Mode == Mode)
            {
                Sender(Id, argument);
            }
        }
    }

    public class NetworkHandler
    {
        public static NetworkHandler Instance = new Lazy<NetworkHandler>(() => new NetworkHandler(), true).Value;

        [Serializable]
        private class Command : Payload
        {
            public string Id;
            public string Payload;

            public Command(string id, string payload)
            {
                Id = id;
                Payload = payload;
            }
        }

        public static ILog Log => Instance.log;
        public static NetworkMode Mode => Instance.commandsHandler == null ? NetworkMode.Server : Instance.commandsHandler.Mode;

        private ILog log;

        private ICommandsHandler commandsHandler;

        private List<Func<object, Reply>> registeredControllers = new List<Func<object, Reply>>();

        private int taskId = 0;

        private Dictionary<NetworkMode, List<Action>> updateActionsDictionary = new Dictionary<NetworkMode, List<Action>>();

        public NetworkHandler()
        {
            foreach (NetworkMode mode in (NetworkMode[])Enum.GetValues(typeof(NetworkMode)))
            {
                updateActionsDictionary.Add(mode, new List<Action>());
            }
        }

        public void Receive(string message)
        {
            if (message == "")
            {
                return;
            }
            if (commandsHandler != null)
            {
                try
                {
                    var command = JsonUtility.FromJson<Command>(message);

                    var id = command.Id;
                    var payload = command.Payload;

                    if (id != "API")
                    {
                        if (id != null && payload != null && TryGetAction(id, out var receiver))
                        {
                            var reply = receiver?.Invoke(payload);
                            if (reply.ReplyStatus != ReplyStatus.Success)
                            {
                                Send("API", reply);
                            }
                        }
                        else
                        {
                            Send("API", Reply.InvalidRequest);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                    Send("API", Reply.ParsingError);
                }
            }
            else
            {
                Send("API", Reply.InternalError);
            }
        }

        public void Register(Func<object, Reply> receiver, out int taskId, out Func<int, object, bool> invoker)
        {
            registeredControllers.Add(receiver);
            taskId = this.taskId;
            this.taskId++;
            invoker = Send;
        }

        public static void RunInUpdate(NetworkMode mode, Action action, UpdateTiming updateTiming)
        {
            Instance.updateActionsDictionary[mode].Add(action);
        }

        public void Deregister<T>(INetworkTask<T> task)
        {
            if (registeredControllers.Count > task.Id)
            {
                registeredControllers.RemoveAt(task.Id);
            }
        }

        public void RegisterCommandsHandler(ICommandsHandler commandsHandler, ILog log)
        {
            if (this.commandsHandler != null)
            {
                this.commandsHandler.UpdateEvent -= Update;
            }
            this.log = log;
            this.commandsHandler = commandsHandler;
            commandsHandler.UpdateEvent += Update;
        }

        private void Update()
        {
            foreach (var action in updateActionsDictionary[Mode])
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                }
            }
        }

        private bool Send(int id, object payload) => Send(id.ToString(), payload);

        private bool Send(string id, object payload)
        {
            if (commandsHandler != null)
            {
                if (TryGetAction(id, out var outAction))
                {
                    var type = payload.GetType();
                    if (type.IsValueType || type.IsPrimitive || type == typeof(string))
                    {
                        commandsHandler.Send(JsonUtility.ToJson(new Command(id, payload.ToString())));
                    }
                    else
                    {
                        commandsHandler.Send(JsonUtility.ToJson(new Command(id, JsonUtility.ToJson(payload))));
                    }
                    return true;
                }
            }
            return false;
        }
        private bool TryGetAction(string taskId, out Func<object, Reply> action)
        {
            action = default;
            try
            {
                int id = Convert.ToInt32(taskId);
                return TryGetAction(id, out action);
            }
            catch (Exception ex)
            {
                Log.LogError(ex);
                return false;
            }
        }

        private bool TryGetAction(int taskId, out Func<object, Reply> action)
        {
            action = default;

            if (registeredControllers.Count > taskId)
            {
                action = registeredControllers[taskId];
                return true;
            }

            return false;
        }
    }
}