using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Aggressors.API
{
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

    public abstract class NetworkBehaviour<M> : MonoBehaviour, IDisposable where M : MonoBehaviour
    {
        private NetworkObject<M> networkObject;

        protected void Register<T>(NetworkMode mode, Func<T, T> func) where T : Payload => networkObject.Register(mode, func);
        protected void Register<T, P>(NetworkMode mode, Func<T, P> func, Func<P, T> toArgument) where P : Payload => networkObject.Register(mode, func, toArgument);
        protected void Invoke<T, P>(Func<T, P> func, T argument) where P : Payload => networkObject.Invoke(func, argument);

        protected abstract void Register();

        protected virtual void Start()
        {
            networkObject = new NetworkObject<M>();
            Register();
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            networkObject.Dispose();
        }
    }

    public class NetworkObject<M> : IDisposable where M : MonoBehaviour
    {
        public NetworkObject()
        {
            networkId = NetworkHandler.RegisterNetworkObject(this).ToString();
        }

        public static string ClassTag => typeof(M).Name;

        private string networkId = "0";

        public void Invoke<T, P>(Func<T, P> func, T argument) where P : Payload => NetworkHandler.Send(ClassTag, networkId, func.Method.Name, func(argument));

        public void Register<T>(NetworkMode mode, Func<T, T> func) where T : Payload => Register(mode, func, p => p);

        public void Register<T, P>(NetworkMode mode, Func<T, P> func, Func<P, T> toArgument) where P : Payload
        {
            NetworkHandler.Register(mode, ClassTag, networkId, func.Method.Name, p =>
             {
                 if (FromJson<P>(p, out var payload))
                 {
                     func(toArgument(payload));
                 }
             });
        }

        private bool FromJson<T>(object json, out T payload) where T : Payload => json.ToString().TryParseJson(out payload);

        public void Dispose()
        {
            NetworkHandler.Deregister(ClassTag, networkId);
        }
    }
}


namespace Aggressors.API
{
    public static class JsonExtensions
    {
        public static bool TryParseJson<T>(this string str, out T result)
        {
            bool success = true;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(str, settings);
            return success;
        }
    }

    public static class ReflectionExtensions
    {
        public static bool HasAttribute<T>(this T member, Type attribute) where T : MemberInfo => member.GetCustomAttributes(attribute, false).Length > 0;

        public static IEnumerable<FieldInfo> GetFields<T>(this T type, BindingFlags flags)
        {
            return type.GetType().GetFields(flags);
        }
    }
}
