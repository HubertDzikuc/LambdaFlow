using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Multiplayer.API
{
    /// <summary>
    /// Can send only the same mode as current mode and recieve only different mode
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
        public static void Send(string classTag, string id, string methodTag, object payload)
        {
            if (commandsHandler != null)
            {
                if (TryGetAction(classTag, id, methodTag, out var action))
                {
                    if (action.mode == commandsHandler.Mode)
                    {
                        commandsHandler.Send(JsonConvert.SerializeObject(new Command(classTag, id, methodTag, payload), Formatting.None));
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
                            if (action.mode != commandsHandler.Mode)
                            {
                                action.action?.Invoke(payload);
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

        public static int RegisterNetworkObject<M>(NetworkObject<M> networkObject) where M : MonoBehaviour
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
                classesNetworkId.Add(tag, 0);
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