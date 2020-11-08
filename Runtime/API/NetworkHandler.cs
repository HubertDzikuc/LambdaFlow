using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public interface ICommandsHandler
    {
        NetworkMode Mode { get; }
        void Send(string command);
    }

    public static class NetworkHandler
    {
        [Serializable]
        private class Command : Payload
        {
            [JsonProperty(PropertyName = "class"), JsonRequired]
            public string ClassTag;
            [JsonProperty(PropertyName = "id"), JsonRequired]
            public string NetworkId;
            [JsonProperty(PropertyName = "method"), JsonRequired]
            public string MethodTag;
            [JsonProperty(PropertyName = "payload"), JsonRequired]
            public object Payload;

            public Command(string classTag, string id, string methodTag, object payload)
            {
                ClassTag = classTag;
                NetworkId = id;
                MethodTag = methodTag;
                Payload = payload;
            }
        }

        public static NetworkMode CurrentMode => commandsHandler == null ? NetworkMode.Server : commandsHandler.Mode;

        private static ICommandsHandler commandsHandler;

        private static Dictionary<string, int> classesNetworkId = new Dictionary<string, int>();

        private static Dictionary<string, Dictionary<string, Dictionary<string, (NetworkMode mode, Action<object> action)>>> registeredControllers = new Dictionary<string, Dictionary<string, Dictionary<string, (NetworkMode mode, Action<object> action)>>>();
        public static void Invoke<T>(string classTag, string id, Action<T> func, T argument) where T : Payload
        {
            if (commandsHandler != null)
            {
                string methodTag = func.Method.Name;
                if (TryGetAction(classTag, id, methodTag, out var action))
                {
                    //If invoke on server, Run and send the 
                    if (commandsHandler.Mode == action.mode)
                    {
                        if (commandsHandler.Mode == NetworkMode.Server)
                        {
                            func(argument);
                            Send(classTag, id, methodTag, argument);
                        }
                        else
                        {
                            Send(classTag, id, methodTag, argument);
                        }
                    }
                }
            }
        }

        public static void Receive(string message)
        {
            if (message == "")
            {
                return;
            }
            if (commandsHandler != null)
            {
                try
                {
                    var commandJson = JObject.Parse(message);
                    var classTag = commandJson["class"].ToString();
                    var id = commandJson["id"].ToString();
                    var methodTag = commandJson["method"].ToString();
                    var payload = commandJson["payload"];

                    if (classTag != "API")
                    {
                        if (TryGetAction(classTag, id, methodTag, out var action))
                        {
                            if (commandsHandler.Mode == NetworkMode.Client)
                            {
                                action.action?.Invoke(payload);
                                return;
                            }
                            else if (commandsHandler.Mode == NetworkMode.Server && action.mode == NetworkMode.Client)
                            {
                                action.action?.Invoke(payload);
                                Send(classTag, id, methodTag, payload);
                                return;
                            }
                            else
                            {
                                Send("API", "", "", Reply.InvalidMode);
                            }
                        }
                        else
                        {
                            Send("API", "", "", Reply.InvalidRequest);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Send("API", "", "", Reply.ParsingError);
                }
            }
            else
            {
                Send("API", "", "", Reply.InternalError);
            }
        }

        public static int RegisterNetworkObject<M>() where M : MonoBehaviour
        {
            var tag = NetworkObject<M>.ClassTag;

            if (classesNetworkId.ContainsKey(tag))
            {
                var id = classesNetworkId[tag];
                classesNetworkId[tag]++;
                return id;
            }
            else
            {
                classesNetworkId.Add(tag, 1);
                return 0;
            }
        }

        public static void Register(NetworkMode mode, string classTag, string id, string methodName, Action<object> action)
        {
            if (!registeredControllers.ContainsKey(classTag))
            {
                registeredControllers.Add(classTag, new Dictionary<string, Dictionary<string, (NetworkMode mode, Action<object> action)>> { { id, new Dictionary<string, (NetworkMode mode, Action<object> action)> { { methodName, (mode, action) } } } });
            }
            else
            {
                if (!registeredControllers[classTag].ContainsKey(id))
                {
                    registeredControllers[classTag].Add(id, new Dictionary<string, (NetworkMode mode, Action<object> action)> { { methodName, (mode, action) } });
                }
                else
                {
                    if (!registeredControllers[classTag][id].ContainsKey(methodName))
                    {
                        registeredControllers[classTag][id].Add(methodName, (mode, action));
                    }
                }
            }
        }

        public static void Deregister(string classTag, string id)
        {
            if (registeredControllers.ContainsKey(classTag))
            {
                if (registeredControllers[classTag].ContainsKey(id))
                {
                    registeredControllers[classTag].Remove(id);
                }
            }
        }

        public static void RegisterCommandsHandler(ICommandsHandler commandsHandler)
        {
            NetworkHandler.commandsHandler = commandsHandler;
        }

        private static void Send(string classTag, string id, string methodTag, object payload)
        {
            if (commandsHandler != null)
            {
                commandsHandler.Send(JsonConvert.SerializeObject(new Command(classTag, id, methodTag, payload), Formatting.None));
            }
        }

        private static bool TryGetAction(string classTag, string id, string methodTag, out (NetworkMode mode, Action<object> action) action)
        {
            action = default;

            if (registeredControllers.TryGetValue(classTag, out var ids))
            {
                if (ids.TryGetValue(id, out var methods))
                {
                    if (methods.TryGetValue(methodTag, out action))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}