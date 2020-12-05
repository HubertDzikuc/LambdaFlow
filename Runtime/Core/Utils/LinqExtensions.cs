using System;
using System.Collections.Generic;

namespace Multiplayer.API.Utils
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
            return enumerable;
        }
    }
}


//public class NetworkObject<T> : IDisposable where T : class
//{

//    public readonly string classTag;
//    public readonly string networkId;
//    private string methodTag => methodId.ToString();

//    private T parent;

//    private int methodId = 0;

//    private Dictionary<NetworkMode, List<Action>> updateActionsDictionary = new Dictionary<NetworkMode, List<Action>>();

//    private List<Action> updateActionsList = new List<Action>();

//    public NetworkObject(T parent)
//    {
//        this.parent = parent;
//        classTag = parent.GetType().Name;
//        networkId = NetworkHandler.Instance.RegisterNetworkObject(this).ToString();
//        foreach (NetworkMode mode in (NetworkMode[])Enum.GetValues(typeof(NetworkMode)))
//        {
//            updateActionsDictionary.Add(mode, new List<Action>());
//        }
//    }

//    public NetworkObject<T> RegisterRequest<P>(Action<P> action, out NetworkLambda<P> invoker) => RegisterNetworkAction(NetworkMode.Client, action, out invoker);
//    public NetworkObject<T> RegisterCommand<P>(Action<P> action, out NetworkLambda<P> invoker) => RegisterNetworkAction(NetworkMode.Server, action, out invoker, false);
//    public NetworkObject<T> RegisterSyncMethod<P>(Action<P> action, out NetworkLambda<P> invoker) => RegisterNetworkAction(NetworkMode.Server, action, out invoker);
//    public NetworkObject<T> RegisterLocalClientMethod<P>(Action<P> action, out LocalAction<P> invoker) => RegisterLocalAction(NetworkMode.Client, action, out invoker);
//    public NetworkObject<T> RegisterLocalServerMethod<P>(Action<P> action, out LocalAction<P> invoker) => RegisterLocalAction(NetworkMode.Server, action, out invoker);

//    public NetworkObject<T> BindToUpdate<P>(NetworkObject<P> networkObject) where P : class => RunOnUpdate(networkObject.Update);

//    public NetworkObject<T> BindToUpdate<P>(NetworkMode mode, NetworkObject<P> networkObject) where P : class => RunOnUpdate(mode, networkObject.Update);

//    public NetworkObject<T> RunOnServerUpdate<P>(NetworkLambda<P> action, Func<P> argument) => RunOnServerUpdate(() => action.Invoke(argument()));
//    public NetworkObject<T> RunOnServerUpdate<P>(LocalAction<P> action, Func<P> argument) => RunOnServerUpdate(() => action.Invoke(argument()));
//    public NetworkObject<T> RunOnClientUpdate<P>(NetworkLambda<P> action, Func<P> argument) => RunOnClientUpdate(() => action.Invoke(argument()));
//    public NetworkObject<T> RunOnClientUpdate<P>(LocalAction<P> action, Func<P> argument) => RunOnClientUpdate(() => action.Invoke(argument()));

//    public NetworkObject<T> RunOnServerUpdate(Action action) => RunOnUpdate(NetworkMode.Server, action);
//    public NetworkObject<T> RunOnClientUpdate(Action action) => RunOnUpdate(NetworkMode.Client, action);

//    private NetworkObject<T> RunOnUpdate(NetworkMode mode, Action action)
//    {
//        updateActionsDictionary[mode].Add(action);
//        return this;
//    }

//    private NetworkObject<T> RunOnUpdate(Action action)
//    {
//        updateActionsList.Add(action);
//        return this;
//    }

//    private NetworkObject<T> RegisterLocalAction<P>(NetworkMode mode, Action<P> action, out LocalAction<P> invoker)
//    {
//        invoker = new LocalAction<P>(mode, action);
//        return this;
//    }

//    private NetworkObject<T> RegisterNetworkAction<P>(NetworkMode mode, Action<P> action, out NetworkLambda<P> invoker, bool call = true)
//    {
//        NetworkHandler.Instance.Register(mode, p =>
//        {
//            if (p.ToString().TryParseJson<P>(out var payload))
//            {
//                action(payload);
//            }
//            else
//            {
//                UnityEngine.Debug.LogWarning($"Couldn't parse data {p} to type {typeof(P)} ({mode} {classTag} {networkId} {action.Method.Name})");
//            }
//        });
//        invoker = new NetworkLambda<P>(mode, call ? action : null);
//        methodId++;
//        return this;
//    }

//    public void Update()
//    {
//        foreach (var action in updateActionsList)
//        {
//            try
//            {
//                action.Invoke();
//            }
//            catch (Exception ex)
//            {
//                UnityEngine.Debug.LogException(ex);
//            }
//        }
//        foreach (var action in updateActionsDictionary[NetworkHandler.CurrentMode])
//        {
//            try
//            {
//                action.Invoke();
//            }
//            catch (Exception ex)
//            {
//                UnityEngine.Debug.LogException(ex);
//            }
//        }
//    }

//    public void Dispose()
//    {
//        NetworkHandler.Deregister(classTag, networkId);
//    }
//}




//public NetworkObject<T> Register<P>(Expression<Func<P>> memberExpression)
//{
//    MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
//    var valueName = expressionBody.Member.Name;
//
//    var field = parent.GetType().GetField(valueName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//
//    var startValue = field.GetValue(parent);
//
//    RegisterCommand(NetworkMode.Server, value =>
//    {
//        startValue = value;
//        field.SetValue(parent, value);
//    }, out Action<P> invoker);
//
//
//    valuesCheck.Add(() =>
//    {
//        var newValue = field.GetValue(parent);
//        if (NetworkHandler.CurrentMode == NetworkMode.Server)
//        {
//            if (newValue != startValue)
//            {
//                startValue = newValue;
//                invoker((P)newValue);
//            }
//        }
//        else
//        {
//            if (newValue != startValue)
//            {
//                field.SetValue(parent, startValue);
//            }
//        }
//    });
//    return this;
//}