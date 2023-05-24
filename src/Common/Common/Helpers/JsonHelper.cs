using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AllSub.Common.Helpers
{
    public static class JsonHelper
    {
        public static string GetJson<T>(T obj)
        {
            if (obj == null)
            {
                return "";
            }

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            string json = JsonConvert.SerializeObject(obj, jsonSerializerSettings);
            return json;
        }

        public static T GetClass<T>(string json)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            T result = JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
            return result;
        }
    }
}
