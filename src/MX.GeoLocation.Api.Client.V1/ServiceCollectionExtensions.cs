using Microsoft.Extensions.DependencyInjection;

using MX.GeoLocation.Api.Client.V1;
using MX.GeoLocation.Abstractions.Interfaces.V1;

using MxIO.ApiClient.Extensions;

namespace MX.GeoLocation.Api.Client.V1
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