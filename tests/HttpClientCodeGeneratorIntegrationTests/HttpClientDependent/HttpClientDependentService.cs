using HttpClientCodeGeneratorIntegrationTests.Models;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClientGenerator.Shared;

namespace HttpClientCodeGeneratorIntegrationTests.HttpClientDependent
{
    public partial class HttpClientDependentService
    {
        private readonly HttpClient _httpClient;

        public HttpClientDependentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("user/{id}")]
        public partial Task<User> GetUserAsync(long id);

    }
}
