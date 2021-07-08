using HttpClientGenerator.Shared;
using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConsoleClientApp
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:5001");
                var userService = new UserHttpService(client);
                var user = await userService.GetUserAsync(1);
                Console.WriteLine($"{user.PhoneNumber}");
            }

            Console.Read();
        }
    }

    public partial class UserHttpService
    {
        [HttpGet("user/{id}")]
        public partial Task<User> GetUserAsync(int id);    
    }

    public class User
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }
    }
}
