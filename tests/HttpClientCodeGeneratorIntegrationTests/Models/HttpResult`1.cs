using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HttpClientCodeGeneratorIntegrationTests.Models
{
    public class HttpResult<T>
    {
        [JsonPropertyName("result")]
        public T Result { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
