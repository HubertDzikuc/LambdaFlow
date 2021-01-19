using System;
using System.Collections.Generic;
using Multiplayer.API.Utils;

namespace Multiplayer.API.Generics
{
    public class SafeEvent<T> where T : Delegate
    {
        public readonly T Invoke;

        private List<T> actions = new List<T>();

        private int currentInvokeIndex = 0;

        public SafeEvent<T> Add(T action)
        {
            actions.Add(action);
            return this;
        }

        public SafeEvent<T> Remove(T action)
        {
            int index = actions.IndexOf(action);
            if (index <= currentInvokeIndex)
            {
                currentInvokeIndex--;
            }
            actions.RemoveAt(index);
            return this;
        }

        public static SafeEvent<T> operator +(SafeEvent<T> evt, T action) => evt.Add(action);
        public static SafeEvent<T> operator -(SafeEvent<T> evt, T action) => evt.Remove(action);

        public SafeEvent()
        {
            Invoke = ExpressionsUtils.GenerateDelegateFromInvoker<T>(this, InvokeAction);
        }

        private void InvokeAction(params object[] arguments)
        {
            while (currentInvokeIndex < actions.Count)
            {
                actions[currentInvokeIndex]?.DynamicInvoke(arguments);
                currentInvokeIndex++;
            }
            currentInvokeIndex = 0;
        }
    }
}