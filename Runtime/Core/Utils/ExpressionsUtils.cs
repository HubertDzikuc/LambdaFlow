using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Multiplayer.API.Utils
{
    public static class ExpressionsUtils
    {
        public static T GenerateDelegateFromInvoker<T>(object obj, Action<object[]> invoker) where T : Delegate
        {
            var parameters = typeof(T).GetMethod("Invoke").GetParameters().Select(p => Expression.Parameter(p.ParameterType)).ToList();

            return Expression.Lambda<T>(Expression.Call(Expression.Constant(obj), invoker.Method, CreateParameterExpressions(parameters)), parameters).Compile();
        }

        public static IEnumerable<Type> ParameterTypesOfDelegate<T>() where T : Delegate
        {
            return typeof(T).GetMethod("Invoke").GetParameters().Select(t => t.ParameterType);
        }

        private static Expression CreateParameterExpressions(IEnumerable<ParameterExpression> parameters)
        {
            return Expression.NewArrayInit(typeof(object), parameters.Select(parameter => Expression.Convert(parameter, typeof(object))));
        }
    }
}