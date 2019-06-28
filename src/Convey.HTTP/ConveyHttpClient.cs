using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;

namespace Convey.HTTP
{
    public class ConveyHttpClient : IHttpClient
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly HttpClient _client;
        private readonly HttpClientOptions _options;
        private readonly ILogger<IHttpClient> _logger;

        public ConveyHttpClient(HttpClient client, HttpClientOptions options, ILogger<IHttpClient> logger)
        {
            _client = client;
            _options = options;
            _logger = logger;
        }

        public virtual Task<T> GetAsync<T>(string requestUri)
            => Policy.Handle<Exception>()
                .WaitAndRetryAsync(_options.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
                .ExecuteAsync(async () =>
                {
                    var uri = requestUri.StartsWith("http://") ? requestUri : $"http://{requestUri}";
                    _logger.LogDebug($"Sending GET HTTP request to URI: {requestUri}");
                    using (var response = await _client.GetAsync(uri))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogDebug($"Received a valid response to GET HTTP request from URI: {uri}" +
                                             $"{Environment.NewLine}{response}");

                            var stream = await response.Content.ReadAsStreamAsync();

                            return DeserializeJsonFromStream<T>(stream);
                        }

                        _logger.LogError($"Received an invalid response to GET HTTP request from URI: {uri}" +
                                         $"{Environment.NewLine}{response}");

                        return default;
                    }
                });

        protected static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
            {
                return default;
            }

            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return JsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}