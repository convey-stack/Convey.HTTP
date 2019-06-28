using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.HTTP
{
    public static class Extensions
    {
        private const string SectionName = "httpClient";
        private const string RegistryName = "httpClient";

        public static IConveyBuilder AddHttpClient(this IConveyBuilder builder, string sectionName = SectionName)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            var options = builder.GetOptions<HttpClient>(sectionName);
            builder.Services.AddSingleton(options);
            builder.Services.AddHttpClient<IHttpClient, ConveyHttpClient>();

            return builder;
        }
    }
}