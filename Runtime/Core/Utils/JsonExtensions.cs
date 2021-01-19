using Newtonsoft.Json;
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
                        var initialResult = JsonConvert.DeserializeObject<T>(str);
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

        public static bool TryParseJson(this string str, Type type, out object result)
        {
            try
            {
                if (type == typeof(string))
                {
                    result = str;
                    return true;
                }
                else if (type.IsValueType || type.IsPrimitive)
                {
                    try
                    {
                        result = Convert.ChangeType(str, type);
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
                    var initialResult = JsonConvert.DeserializeObject(str, type);
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
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}

