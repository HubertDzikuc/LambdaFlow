using UnityEngine;

namespace Multiplayer.API
{
    public static class JsonExtensions
    {
        public static bool TryParseJson<T>(this string str, out T result)
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
}
