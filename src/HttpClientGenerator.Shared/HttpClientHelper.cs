using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace HttpClientGenerator.Shared
{
    public static class HttpClientHelper
    {
        /// <summary>
        /// Represents the default serializer options for all generated HttpClient APIs.
        /// </summary>
        public static JsonSerializerOptions DefaultJsonSerializerOptions { get; } = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static async Task SendDataAsync<TRequest>(
            HttpClient httpClient, string method, string path, Dictionary<string, string> headers = null,
            Dictionary<string, object> routeParams = null, Dictionary<string, object> queryStringParams = null,
            TRequest requestModel = default, bool ensureSuccessStatusCode = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var computedPath = ComputePath(path, routeParams, queryStringParams);

            var request = CreateRequest(method, computedPath, requestModel, headers);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (ensureSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public static async Task<TResponse> SendDataAsync<TRequest, TResponse>(
            HttpClient httpClient, string method, string path, Dictionary<string, string> headers = null,
            Dictionary<string, object> routeParams = null, Dictionary<string, object> queryStringParams = null,
            TRequest requestModel = default, bool ensureSuccessStatusCode = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var computedPath = ComputePath(path, routeParams, queryStringParams);

            var request = CreateRequest(method, computedPath, requestModel, headers);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (ensureSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return Deserialize<TResponse>(responseContent);
        }

        public static async Task<TResponse> SendAsync<TResponse>(
            HttpClient httpClient, string method, string path, Dictionary<string, string> headers = null,
            Dictionary<string, object> routeParams = null, Dictionary<string, object> queryStringParams = null,
            bool ensureSuccessStatusCode = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var computedPath = ComputePath(path, routeParams, queryStringParams);

            var request = CreateRequest(method, computedPath, default, headers);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (ensureSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return Deserialize<TResponse>(responseContent);
        }

        public static async Task SendAsync(
            HttpClient httpClient, string method, string path, Dictionary<string, string> headers = null,
            Dictionary<string, object> routeParams = null, Dictionary<string, object> queryStringParams = null,
            bool ensureSuccessStatusCode = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            var computedPath = ComputePath(path, routeParams, queryStringParams);

            var request = CreateRequest(method, computedPath, default, headers);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (ensureSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }
        }

        private static string ComputePath(string path, Dictionary<string, object> routeParams = null, Dictionary<string, object> queryStringParams = null)
        {
            var computedPath = path.TrimEnd('?', ' ', '/');
            if (routeParams != null && routeParams.Any())
            {
                foreach (var routeParam in routeParams)
                {
                    computedPath = computedPath.Replace($"{{{routeParam.Key}}}", routeParam.Value?.ToString());
                }
            }

            if (queryStringParams != null && queryStringParams.Any())
            {
                computedPath += "?";
                foreach (var qsParam in queryStringParams)
                {
                    var encodedValue = HttpUtility.UrlEncode(qsParam.Value?.ToString() ?? string.Empty);
                    computedPath += $"{qsParam.Key}={encodedValue}&";
                }
            }

            return computedPath.TrimEnd('&');
        }

        private static HttpRequestMessage CreateRequest(string method, string path, object requestModel, Dictionary<string, string> headers)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), path);
            if (requestModel != null)
            {
                request.Content = new StringContent(SerializeJson(requestModel), Encoding.UTF8, "application/json");
            }

            if (headers != null && headers.Any())
            {
                foreach (var headerKvp in headers)
                {
                    request.Headers.Add(headerKvp.Key, headerKvp.Value);
                }
            }

            return request;
        }

        private static string SerializeJson(object obj) => JsonSerializer.Serialize(obj, DefaultJsonSerializerOptions);

        private static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, DefaultJsonSerializerOptions);
    }
}

