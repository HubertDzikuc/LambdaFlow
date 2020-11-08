using Newtonsoft.Json;

namespace Multiplayer.API
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
}
