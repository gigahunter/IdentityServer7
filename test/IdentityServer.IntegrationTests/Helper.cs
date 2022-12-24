using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace IdentityServer.IntegrationTests
{
    internal static class Helper
    {
        public static T? ToObject<T>(this JsonElement element)
        {
            var json = element.GetRawText();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T? ToObject<T>(this JsonDocument document)
        {
            return document.RootElement.ToObject<T>();
        }

        public static JArray? ToArray(this JsonElement element, string prop)
        {
            var elm = element.GetProperty(prop);
            var json = elm.GetRawText();
            return JsonConvert.DeserializeObject<JArray>(json);
        }
    }
}