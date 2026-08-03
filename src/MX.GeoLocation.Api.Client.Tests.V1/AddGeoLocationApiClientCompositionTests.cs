using System;

using Microsoft.Extensions.DependencyInjection;

using MX.Api.Abstractions;
using MX.Api.Client.Configuration;

using MX.GeoLocation.Abstractions.Interfaces;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Api.Client.V1;

using V1_1 = MX.GeoLocation.Abstractions.Interfaces.V1_1;

namespace MX.GeoLocation.Api.Client.Tests.V1;

/// <summary>
/// DI-composition regression tests for <see cref="ServiceCollectionExtensions.AddGeoLocationApiClient"/>.
/// These lock in the shared-cache scoping fix — a consumer <c>WithCaching</c> delegate that targets
/// multiple typed sub-APIs must not throw <see cref="ArgumentException"/> during registration when the
/// same builder configuration is re-applied per typed client by MX.Api.Client.
/// </summary>
[Trait("Category", "Unit")]
public class AddGeoLocationApiClientCompositionTests
{
    // Bogus interface used to force ValidateAllOperationsMatched() to detect a typo.
    private interface INotRegisteredApi
    {
        System.Threading.Tasks.Task<ApiResult<GeoLocationDto>> Bogus(string hostname, System.Threading.CancellationToken ct = default);
    }

    [Fact]
    public void AddGeoLocationApiClient_WithMultiSubApiCache_ResolvesEverySubApi()
    {
        // Arrange — a consumer delegate that captures cache expressions for BOTH the V1 and V1.1
        // sub-APIs in a single .WithCaching(...). Before the SharedCacheConfiguration fix this
        // would throw ArgumentException at BuildServiceProvider time because MX.Api.Client scoped
        // WithCaching to whichever typed client was being registered at that moment.
        var services = new ServiceCollection();

        services.AddGeoLocationApiClient(options => options
            .WithBaseUrl("https://example.invalid")
            .WithApiKeyAuthentication("test-key")
            .WithCachePartition("unit-tests")
            .WithCaching(cache => cache
                .UseLibraryDefaults()
                .InMemory<IGeoLookupApi, System.Threading.Tasks.Task<ApiResult<GeoLocationDto>>>(
                    api => api.GetGeoLocation(default!, default),
                    TimeSpan.FromMinutes(30))
                .InMemory<V1_1.IGeoLookupApi, System.Threading.Tasks.Task<ApiResult<MX.GeoLocation.Abstractions.Models.V1_1.CityGeoLocationDto>>>(
                    api => api.GetCityGeoLocation(default!, default),
                    TimeSpan.FromMinutes(30))));

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // Act + Assert — every registered sub-API and the unified client must resolve.
        Assert.NotNull(sp.GetRequiredService<IGeoLookupApi>());
        Assert.NotNull(sp.GetRequiredService<V1_1.IGeoLookupApi>());
        Assert.NotNull(sp.GetRequiredService<IApiInfoApi>());
        Assert.NotNull(sp.GetRequiredService<IApiHealthApi>());
        Assert.NotNull(sp.GetRequiredService<IGeoLocationApiClient>());
    }

    [Fact]
    public void AddGeoLocationApiClient_WithoutCaching_ResolvesEverySubApi()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddGeoLocationApiClient(options => options
            .WithBaseUrl("https://example.invalid")
            .WithApiKeyAuthentication("test-key"));

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // Act + Assert
        Assert.NotNull(sp.GetRequiredService<IGeoLookupApi>());
        Assert.NotNull(sp.GetRequiredService<V1_1.IGeoLookupApi>());
        Assert.NotNull(sp.GetRequiredService<IApiInfoApi>());
        Assert.NotNull(sp.GetRequiredService<IApiHealthApi>());
        Assert.NotNull(sp.GetRequiredService<IGeoLocationApiClient>());
    }

    [Fact]
    public void AddGeoLocationApiClient_WithCacheExpressionForUnknownInterface_ThrowsInvalidOperation()
    {
        // Arrange — a captured expression targets an interface that isn't a registered sub-API.
        // SharedCacheConfiguration.ValidateAllOperationsMatched() must surface this as an
        // InvalidOperationException so consumers catch typos at registration time.
        var services = new ServiceCollection();

        Action act = Register;

        // Act + Assert
        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("shared cache operations", ex.Message, StringComparison.OrdinalIgnoreCase);

        void Register()
        {
            services.AddGeoLocationApiClient(options => options
                .WithBaseUrl("https://example.invalid")
                .WithApiKeyAuthentication("test-key")
                .WithCaching(cache => cache.InMemory<INotRegisteredApi, System.Threading.Tasks.Task<ApiResult<GeoLocationDto>>>(
                    api => api.Bogus(default!, default),
                    TimeSpan.FromMinutes(1))));
        }
    }
}
