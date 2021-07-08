using HttpClientCodeGeneratorIntegrationTests.Models;
using System.Threading.Tasks;
using HttpClientGenerator.Shared;
using System.Collections.Generic;

namespace HttpClientCodeGeneratorIntegrationTests
{
    public partial class MyHttpClient
    {

        [HttpGet("user/{id}")]
        public partial Task<User> GetUserAsync(long id);

        [HttpGet("user/wrapped/{id}")]
        public partial Task<HttpResult<User>> GetWrappedUserAsync(long id);

        [HttpGet("user/search")]
        public partial Task<IEnumerable<User>> SearchUserByNameAsync(string name);

        [HttpPost("user")]
        public partial Task<User> CreateUser(User user);

        [HttpPut("user/{id}")]
        public partial Task<User> UpdateUserAsync(int id, User user);

        [HttpDelete("user")]
        public partial Task RemoveUserAsync(int id);
    }
}
