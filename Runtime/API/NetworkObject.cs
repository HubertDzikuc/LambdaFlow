using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Multiplayer.API
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


    public class NetworkObject<T> : IDisposable where T : class
    {
        public NetworkObject(T parent)
        {
            this.parent = parent;
            ClassTag = parent.GetType().Name;
            NetworkId = NetworkHandler.RegisterNetworkObject(this).ToString();
        }

        public string ClassTag { get; private set; } = "";
        public string NetworkId { get; private set; }

        private T parent;

        private List<Action> valuesCheck = new List<Action>();

        public NetworkObject<T> Invoke<P>(Action<P> action, P argument)
        {
            NetworkHandler.Invoke(ClassTag, NetworkId, action, argument);
            return this;
        }

        public NetworkObject<T> Register<P>(NetworkMode mode, Action<P> action)
        {
            return Register(mode, action, out var invoker);
        }

        public NetworkObject<T> Register<P>(Expression<Func<P>> memberExpression)
        {
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            var valueName = expressionBody.Member.Name;

            var field = parent.GetType().GetField(valueName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var startValue = field.GetValue(parent);

            Register(NetworkMode.Server, value =>
            {
                startValue = value;
                field.SetValue(parent, value);
            }, out Action<P> invoker);


            valuesCheck.Add(() =>
            {
                var newValue = field.GetValue(parent);
                if (NetworkHandler.CurrentMode == NetworkMode.Server)
                {
                    if (newValue != startValue)
                    {
                        startValue = newValue;
                        invoker((P)newValue);
                    }
                }
                else
                {
                    if (newValue != startValue)
                    {
                        field.SetValue(parent, startValue);
                    }
                }
            });
            return this;
        }

        public NetworkObject<T> Register<P>(NetworkMode mode, Action<P> action, out Action<P> invoker)
        {
            NetworkHandler.Register(mode, ClassTag, NetworkId, action.Method.Name, p =>
            {
                if (p.ToString().TryParseJson<P>(out var payload))
                {
                    action(payload);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Couldn't parse data {p} to type {typeof(P)} ({mode} {ClassTag} {NetworkId} {action.Method.Name})");
                }
            });

            invoker = argument =>
            {
                Invoke(action, argument);
            };

            return this;
        }

        public virtual void Update()
        {
            foreach (var valueCheck in valuesCheck)
            {
                valueCheck();
            }
        }

        public void Dispose()
        {
            NetworkHandler.Unregister(ClassTag, NetworkId);
        }
    }
}
