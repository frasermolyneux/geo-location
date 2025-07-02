using Microsoft.Extensions.DependencyInjection;

using MX.GeoLocation.GeoLocationApi.Client.Api;
using MX.GeoLocation.LookupApi.Abstractions.Interfaces;

using MxIO.ApiClient.Extensions;

namespace MX.GeoLocation.GeoLocationApi.Client
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGeoLocationApiClient(this IServiceCollection serviceCollection,
            Action<GeoLocationApiClientOptions> configure)
        {
            serviceCollection.AddApiClient();

            serviceCollection.Configure(configure);

            serviceCollection.AddSingleton<IGeoLookupApi, GeoLookupApi>();

            serviceCollection.AddSingleton<IGeoLocationApiClient, GeoLocationApiClient>();
        }
    }
}