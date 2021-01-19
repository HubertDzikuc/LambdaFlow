using Multiplayer.API.Generics;
using Multiplayer.API.Payloads;
using Newtonsoft.Json;
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

        private List<Func<object[], Reply>> registeredControllers = new List<Func<object[], Reply>>();

        private Dictionary<NetworkMode, SafeEvent<Action>> updateActionsDictionary = new Dictionary<NetworkMode, SafeEvent<Action>>();

        private static int lambdaId = 0;

        public NetworkHandler()
        {
            foreach (NetworkMode mode in (NetworkMode[])Enum.GetValues(typeof(NetworkMode)))
            {
                updateActionsDictionary.Add(mode, new SafeEvent<Action>());
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
            this.commandsHandler.UpdateEvent += Update;

            this.commandsHandler.ReceiveEvent += Receive;
        }

        public void Register(Func<object[], Reply> receiver, out int lambdaId, out Func<int, object[], bool> sender, out Action deregister)
        {
            registeredControllers.Add(receiver);
            lambdaId = NetworkHandler.lambdaId;
            deregister = () => Deregister(NetworkHandler.lambdaId);
            NetworkHandler.lambdaId++;
            sender = Send;
        }

        public static void RunOnUpdate(NetworkMode mode, Action action, UpdateTiming updateTiming)
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
                    var command = JsonConvert.DeserializeObject<Command>(message);

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
                            Send("API", Reply.InvalidRequest());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                    Send("API", Reply.ParsingError());
                }
            }
            else
            {
                Send("API", Reply.InternalError());
            }
        }

        private void Update()
        {
            try
            {
                updateActionsDictionary[Mode].Invoke();
            }
            catch (Exception ex)
            {
                Log.LogError(ex);
            }
        }

        private bool Send(int id, params object[] payload) => Send(id.ToString(), payload);

        private bool Send(string id, params object[] payload)
        {
            if (commandsHandler != null)
            {
                var type = payload.GetType();

                commandsHandler.Send(JsonConvert.SerializeObject(new Command(id, payload)));

                return true;
            }
            return false;
        }

        private bool TryGetAction(string lambdaId, out Func<object[], Reply> action)
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

        private bool TryGetAction(int lambdaId, out Func<object[], Reply> action)
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