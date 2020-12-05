using System;
using UnityEngine;

namespace Multiplayer.API.Utils
{
    public static class JsonExtensions
    {
        public static bool TryParseJson<T>(this string str, out T result)
        {
            try
            {
                var type = typeof(T);
                if (type.IsValueType || type.IsPrimitive || type == typeof(string))
                {
                    try
                    {
                        result = (T)Convert.ChangeType(str, typeof(T));
                        return true;
                    }
                    catch
                    {
                        result = default;
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        var initialResult = JsonUtility.FromJson<T>(str);
                        if (initialResult != null)
                        {
                            result = initialResult;
                            return true;
                        }
                        else
                        {
                            result = default;
                            return false;
                        }
                    }
                    catch
                    {
                        result = default;
                        return false;
                    }
                }
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}

