using Xunit;
using Xunit.Abstractions;

namespace HttpClientCodeGeneratorTests
{
    public class AdHocTests : Internals.TestBase
    {
        private readonly ITestOutputHelper _output;

        public AdHocTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void MethodProvidedHttpClient()
        {
            string source = @"
using System;
using System.Threading.Tasks;
using System.Net.Http;
using HttpClientGenerator;
using HttpClientGenerator.Shared;

namespace ConsoleClientApp
{
    public partial class MyHttpClient
    {
        public HttpClient GetHttpClient() { return null; }

        [HttpGet(""/api/v1/user/{id}"")]
        public partial Task<User> GetUserAsync(int id);
    }
}
namespace ConsoleClientApp.Models
{
    public class User {}
}
";

            string output = GetGeneratedOutput(source);

        }


        [Fact]
        public void FieldProvidedHttpClient()
        {
            string source = @"
using System;
using System.Threading.Tasks;
using System.Net.Http;
using HttpClientGenerator;
using HttpClientGenerator.Shared;

namespace ConsoleClientApp
{
    public partial class MyHttpClient
    {
        private readonly HttpClient _httpClient;

        public HttpClientDependentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet(""/api/v1/user/{id}"")]
        public partial Task<User> GetUserAsync(int id);
    }
}
namespace ConsoleClientApp.Models
{
    public class User {}
}
";

            string output = GetGeneratedOutput(source);

        }

        [Fact]
        public void NoHttpClientFieldProvidedHttpClient()
        {
            string source = @"
using HttpClientCodeGeneratorIntegrationTests.Models;
using System.Threading.Tasks;
using HttpClientGenerator.Shared;
using System.Collections.Generic;

namespace HttpClientCodeGeneratorIntegrationTests.Basics
{
    public partial class MyHttpService
    {
        [HttpGet(""user/{id}"")]
        public partial Task<User> GetUserAsync(long id);

        [HttpGet(""user/wrapped/{id}"")]
        public partial Task<HttpResult<User>> GetWrappedUserAsync(long id);

        [HttpGet(""user/search"")]
        public partial Task<IEnumerable<User>> SearchUserByNameAsync(string name);

        [HttpPost(""user"")]
        public partial Task<User> CreateUser(User user);

        [HttpPut(""user/{id}"")]
        public partial Task<User> UpdateUserAsync(int id, User user);

        [HttpDelete(""user/{id}"")]
        public partial Task RemoveUserAsync(int id);
    }
}
namespace ConsoleClientApp.Models
{
    public class User {}
}
";

            string output = GetGeneratedOutput(source);

        }
    }
}
