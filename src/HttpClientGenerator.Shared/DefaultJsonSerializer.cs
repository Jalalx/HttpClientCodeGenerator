using System.Text.Json;

namespace HttpClientGenerator.Shared
{
    public class DefaultJsonSerializer : IObjectSerializer
    {
        static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public string Serialize(object obj)
        {
            if (obj == null) return string.Empty;
            return JsonSerializer.Serialize(obj, options);
        }

        public T Deserialize<T>(string content)
        {
            return JsonSerializer.Deserialize<T>(content, options);
        }
    }
}

