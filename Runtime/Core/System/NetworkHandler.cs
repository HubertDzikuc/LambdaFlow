using Multiplayer.API.Payloads;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer.API.System
{
    public class NetworkHandler
    {
        public static NetworkHandler Instance = new Lazy<NetworkHandler>(() => new NetworkHandler(), true).Value;
        public static ILog Log => Instance.log;
        public static NetworkMode Mode => Instance.commandsHandler == null ? NetworkMode.Server : Instance.commandsHandler.Mode;

        private ILog log;

        private ICommandsHandler commandsHandler;

        private List<Func<object, Reply>> registeredControllers = new List<Func<object, Reply>>();

        private Dictionary<NetworkMode, List<Action>> updateActionsDictionary = new Dictionary<NetworkMode, List<Action>>();

        private static int lambdaId = 0;

        public NetworkHandler()
        {
            foreach (NetworkMode mode in (NetworkMode[])Enum.GetValues(typeof(NetworkMode)))
            {
                updateActionsDictionary.Add(mode, new List<Action>());
            }
        }

        public static bool IsMode(params NetworkMode[] modes)
        {
            foreach (var mode in modes)
            {
                if (Mode == mode)
                {
                    return true;
                }
            }
            return false;
        }

        public void RegisterCommandsHandler(ICommandsHandler commandsHandler)
        {
            if (this.commandsHandler != null)
            {
                this.commandsHandler.UpdateEvent -= Update;
            }
            this.commandsHandler = commandsHandler;
            this.log = commandsHandler.Logger;
            commandsHandler.UpdateEvent += Update;
            commandsHandler.ReceiveEvent += Receive;
        }

        public void Register(Func<object, Reply> receiver, out int lambdaId, out Func<int, object, bool> sender, out Action deregister)
        {
            registeredControllers.Add(receiver);
            lambdaId = NetworkHandler.lambdaId;
            deregister = () => Deregister(NetworkHandler.lambdaId);
            NetworkHandler.lambdaId++;
            sender = Send;
        }

        public static void RunInUpdate(NetworkMode mode, Action action, UpdateTiming updateTiming)
        {
            Instance.updateActionsDictionary[mode].Add(action);
        }

        private void Deregister(int id)
        {
            if (registeredControllers.Count > id)
            {
                registeredControllers.RemoveAt(id);
            }
        }

        private void Receive(string message)
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
            return false;
        }

        private bool TryGetAction(string lambdaId, out Func<object, Reply> action)
        {
            action = default;
            try
            {
                int id = Convert.ToInt32(lambdaId);
                return TryGetAction(id, out action);
            }
            catch (Exception ex)
            {
                Log.LogError(ex);
                return false;
            }
        }

        private bool TryGetAction(int lambdaId, out Func<object, Reply> action)
        {
            action = default;

            if (registeredControllers.Count > lambdaId)
            {
                action = registeredControllers[lambdaId];
                return true;
            }

            return false;
        }
    }
}