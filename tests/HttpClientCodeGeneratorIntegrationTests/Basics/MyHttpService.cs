using HttpClientCodeGeneratorIntegrationTests.Models;
using System.Threading.Tasks;
using HttpClientGenerator.Shared;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace HttpClientCodeGeneratorIntegrationTests.Basics
{
    public partial class MyHttpService
    {
        public MyHttpService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        [HttpGet("user/{id}")]
        public partial Task<User> GetUserAsync(long id, CancellationToken cancellationToken);

        [HttpGet("user/wrapped/{id}")]
        public partial Task<HttpResult<User>> GetWrappedUserAsync(long id);

        [HttpGet("user/search")]
        public partial Task<IEnumerable<User>> SearchUserByNameAsync(string name);

        [HttpGet("user/by-name/{name}")]
        public partial Task<User> GetUserByNameAsync(string name);

        [HttpPost("user")]
        public partial Task<User> CreateUser(User user);

        [HttpPut("user/{id}")]
        public partial Task<User> UpdateUserAsync(int id, User user);

        [HttpDelete("user/{id}")]
        public partial Task RemoveUserAsync(int id);
    }
}
