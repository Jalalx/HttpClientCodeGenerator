using Microsoft.AspNetCore.Mvc.Testing;
using SampleRestApi;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using HttpClientGenerator.Shared;

namespace HttpClientCodeGeneratorIntegrationTests
{
    public class MyHttpClientBasicScenarioTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public MyHttpClientBasicScenarioTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetUser_ByValidId_ReturnsExpectedValue()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpClient(client);

            // Act
            var user = await myClient.GetUserAsync(1);

            // Assert
            Assert.Equal("+981234567", user.PhoneNumber);
        }

        [Fact]
        public async Task SearchUser_ByValidName_ReturnsExpectedValue()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpClient(client);

            // Act
            var users = await myClient.SearchUserByNameAsync("will");

            // Assert
            Assert.Single(users);
            Assert.Equal("+981234567", users.First().PhoneNumber);
        }

        [Fact]
        public async Task GetWrappedUser_ByValidId_ReturnsExpectedWrappedValue()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpClient(client);

            // Act
            var user = await myClient.GetWrappedUserAsync(1);

            // Assert
            Assert.NotNull(user.Result);
            Assert.Equal("+981234567", user.Result.PhoneNumber);
        }

        [Fact]
        public async Task PostUser_ByValidParams_ActsAsExpected()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpClient(client);

            // Act
            var user = new Models.User { FirstName = "Bill", LastName = "Gates", PhoneNumber = "+16651345" };
            var createdUser = await myClient.CreateUser(user);

            // Assert
            Assert.NotNull(createdUser);
            Assert.Equal("+16651345", createdUser.PhoneNumber);
        }
    }
}
