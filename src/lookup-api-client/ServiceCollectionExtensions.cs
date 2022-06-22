using Microsoft.Extensions.DependencyInjection;

using MX.GeoLocation.GeoLocationApi.Client.Api;
using MX.GeoLocation.LookupApi.Abstractions.Interfaces;

namespace MX.GeoLocation.GeoLocationApi.Client
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServersApiClient(this IServiceCollection serviceCollection,
            Action<GeoLocationApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);

            serviceCollection.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

            serviceCollection.AddSingleton<IGeoLookupApi, GeoLookupApi>();

            serviceCollection.AddSingleton<IGeoLocationApiClient, GeoLocationApiClient>();
        }
    }
}