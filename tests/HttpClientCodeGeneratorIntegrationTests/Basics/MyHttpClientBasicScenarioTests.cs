using Microsoft.AspNetCore.Mvc.Testing;
using SampleRestApi;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HttpClientCodeGeneratorIntegrationTests.Basics
{
    public class MyHttpServiceBasicScenarioTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;


        public MyHttpServiceBasicScenarioTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }



        // Fetch by id
        [Fact]
        public async Task GetUser_ByValidId_ReturnsExpectedValue()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var user = await myClient.GetUserAsync(1, CancellationToken.None);

            // Assert
            Assert.Equal("Will", user.FirstName);
            Assert.Equal("Smith", user.LastName);
            Assert.Equal("+981234567", user.PhoneNumber);
        }

        // Fetch by name
        [Fact]
        public async Task GetUser_ByValidName_ReturnsExpectedValue()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var user = await myClient.GetUserByNameAsync("Will");

            // Assert
            Assert.Equal("Will", user.FirstName);
            Assert.Equal("Smith", user.LastName);
            Assert.Equal("+981234567", user.PhoneNumber);
        }

        [Fact]
        public async Task GetUser_ByInvalidId_Throws404HttpRequestException()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var ex = await Record.ExceptionAsync(async () => await myClient.GetUserAsync(-1, CancellationToken.None));

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<HttpRequestException>(ex);
            Assert.Equal(HttpStatusCode.NotFound, (ex as HttpRequestException).StatusCode);
        }



        // Search user
        [Fact]
        public async Task SearchUserByNameAsync_ByExistingName_ReturnsExpectedValue()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var users = await myClient.SearchUserByNameAsync("will");

            // Assert
            Assert.Single(users);
            Assert.Equal("Will", users.First().FirstName);
            Assert.Equal("Smith", users.First().LastName);
            Assert.Equal("+981234567", users.First().PhoneNumber);
        }

        [Fact]
        public async Task SearchUserByNameAsync_ByNonExistingName_ReturnsEmptyResult()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var users = await myClient.SearchUserByNameAsync("jim");

            // Assert
            Assert.Empty(users);
        }



        // Get by wrapper model
        [Fact]
        public async Task GetWrappedUser_ByValidId_ReturnsExpectedWrappedValue()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var user = await myClient.GetWrappedUserAsync(1);

            // Assert
            Assert.NotNull(user.Result);
            Assert.Equal("Will", user.Result.FirstName);
            Assert.Equal("Smith", user.Result.LastName);
            Assert.Equal("+981234567", user.Result.PhoneNumber);
        }



        // Posting a model
        [Fact]
        public async Task CreateUser_ByValidParams_ActsAsExpected()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var user = new Models.User { FirstName = "Bill", LastName = "Gates", PhoneNumber = "+16651345" };
            var createdUser = await myClient.CreateUser(user);

            // Assert
            Assert.NotNull(createdUser);
            Assert.Equal(user.FirstName, createdUser.FirstName);
            Assert.Equal(user.LastName, createdUser.LastName);
            Assert.Equal(user.PhoneNumber, createdUser.PhoneNumber);
        }

        [Fact]
        public async Task CreateUser_BySomeInvalidParams_Thorws400HttpRequestException()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var user = new Models.User { FirstName = "", LastName = null, PhoneNumber = "" };
            var ex = await Record.ExceptionAsync(async () => await myClient.CreateUser(user));

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<HttpRequestException>(ex);
            Assert.Equal(HttpStatusCode.BadRequest, (ex as HttpRequestException).StatusCode);
        }



        // Putting a model
        [Fact]
        public async Task UpdateUser_ByValidParams_ActsAsExpected()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var user = new Models.User { FirstName = "Jon", LastName = "Doe", PhoneNumber = "+16699999" };
            var createdUser = await myClient.UpdateUserAsync(2, user);

            // Assert
            Assert.NotNull(createdUser);
            Assert.Equal(user.FirstName, createdUser.FirstName);
            Assert.Equal(user.LastName, createdUser.LastName);
            Assert.Equal(user.PhoneNumber, createdUser.PhoneNumber);
        }

        [Fact]
        public async Task UpdateUser_BySomeInvalidParams_Thorws400HttpRequestException()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var user = new Models.User { FirstName = null, LastName = null, PhoneNumber = "+16699999" };
            var ex = await Record.ExceptionAsync(async () => await myClient.UpdateUserAsync(2, user));

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<HttpRequestException>(ex);
            Assert.Equal(HttpStatusCode.BadRequest, (ex as HttpRequestException).StatusCode);
        }



        // Deleting a model
        [Fact]
        public async Task RemoveUser_ByValidId_ActsAsExpected()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var ex = await Record.ExceptionAsync(async () => await myClient.RemoveUserAsync(3));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public async Task RemoveUser_ByInvalidId_Thorws404HttpRequestException()
        {
            // Arrange
            var client = _factory.CreateClient();
            var myClient = new MyHttpService(client);

            // Act
            var ex = await Record.ExceptionAsync(async () => await myClient.RemoveUserAsync(-3));

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<HttpRequestException>(ex);
            Assert.Equal(HttpStatusCode.NotFound, (ex as HttpRequestException).StatusCode);
        }
    }
}
