# Testing with the GeoLocation API Client

The `MX.GeoLocation.Api.Client.Testing` NuGet package provides in-memory fakes and factory helpers so consumer apps can test against `IGeoLocationApiClient` without Moq or any mocking framework.

## Installation

```bash
dotnet add package MX.GeoLocation.Api.Client.Testing
```

Add this to your test project only — it should not be referenced from production code.

## The Problem

The `IGeoLocationApiClient` interface has a nested structure:

```
IGeoLocationApiClient
├── .GeoLookup (IVersionedGeoLookupApi)
│   ├── .V1 (IGeoLookupApi)          → GetGeoLocation, GetGeoLocations, DeleteMetadata
│   └── .V1_1 (IGeoLookupApi)        → GetCityGeoLocation, GetInsightsGeoLocation
├── .ApiInfo (IApiInfoApi)            → GetApiInfo
└── .ApiHealth (IApiHealthApi)        → CheckHealth
```

Without the testing package, each test needs 3+ levels of nested mocks just to call a single method. Additionally, all DTO properties use `internal set`, so external consumers cannot construct DTOs with custom values.

## Unit Tests

Use `FakeGeoLocationApiClient` as a direct replacement — no mocking framework needed:

```csharp
using MX.GeoLocation.Api.Client.Testing;

[Fact]
public async Task MyService_UsesGeoLocation()
{
    // Arrange — configure canned responses
    var fakeClient = new FakeGeoLocationApiClient();
    fakeClient.V1Lookup.AddResponse("8.8.8.8",
        GeoLocationDtoFactory.CreateGeoLocation(
            address: "8.8.8.8",
            cityName: "Mountain View",
            countryName: "United States"));

    var service = new MyService(fakeClient);

    // Act
    var result = await service.LookupPlayer("8.8.8.8");

    // Assert
    Assert.Equal("Mountain View", result.City);
}
```

### Unconfigured addresses

Addresses without a registered response return a default DTO with `"Test City"` / `"Test Country"` and the requested address — tests won't throw, but you can assert against known values.

### V1.1 endpoints

```csharp
fakeClient.V1_1Lookup.AddCityResponse("1.1.1.1",
    GeoLocationDtoFactory.CreateCityGeoLocation(
        cityName: "Los Angeles",
        subdivisions: ["California"],
        networkTraits: GeoLocationDtoFactory.CreateNetworkTraits(isp: "Cloudflare")));

fakeClient.V1_1Lookup.AddInsightsResponse("10.0.0.1",
    GeoLocationDtoFactory.CreateInsightsGeoLocation(
        cityName: "Amsterdam",
        anonymizer: GeoLocationDtoFactory.CreateAnonymizer(isAnonymousVpn: true)));
```

### Verifying deletes

```csharp
await fakeClient.V1Lookup.DeleteMetadata("8.8.8.8");

Assert.Contains("8.8.8.8", fakeClient.V1Lookup.DeletedAddresses);
```

## Integration Tests (WebApplicationFactory)

Use `AddFakeGeoLocationApiClient()` to replace the real client in your DI container:

```csharp
using MX.GeoLocation.Api.Client.Testing;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddFakeGeoLocationApiClient(client =>
            {
                client.V1Lookup.AddResponse("8.8.8.8",
                    GeoLocationDtoFactory.CreateGeoLocation(
                        cityName: "Mountain View",
                        countryName: "United States"));
                client.V1_1Lookup.AddCityResponse("1.1.1.1",
                    GeoLocationDtoFactory.CreateCityGeoLocation(
                        cityName: "Los Angeles"));
            });
        });
    }
}
```

This replaces all geo-location service registrations (`IGeoLocationApiClient`, `IVersionedGeoLookupApi`, `IGeoLookupApi`, etc.) with fakes in a single call.

## Factory Methods Reference

All factory methods use named parameters with sensible defaults — only specify what your test cares about.

| Method | Returns | Key Parameters |
|---|---|---|
| `CreateGeoLocation(...)` | `GeoLocationDto` | `address`, `cityName`, `countryName`, `countryCode`, `latitude`, `longitude` |
| `CreateCityGeoLocation(...)` | `CityGeoLocationDto` | Same as above + `subdivisions`, `networkTraits` |
| `CreateInsightsGeoLocation(...)` | `InsightsGeoLocationDto` | Same as city + `anonymizer` |
| `CreateNetworkTraits(...)` | `NetworkTraitsDto` | `autonomousSystemNumber`, `isp`, `organization`, `domain`, `connectionType` |
| `CreateAnonymizer(...)` | `AnonymizerDto` | `isAnonymous`, `isAnonymousVpn`, `isTorExitNode`, `isHostingProvider`, `isPublicProxy` |
| `CreateApiInfo(...)` | `ApiInfoDto` | `version`, `buildVersion`, `assemblyVersion` |

## Fake Classes Reference

| Class | Implements | Key Members |
|---|---|---|
| `FakeGeoLocationApiClient` | `IGeoLocationApiClient` | `.V1Lookup`, `.V1_1Lookup`, `.InfoApi`, `.HealthApi` |
| `FakeGeoLookupApi` | `IGeoLookupApi` (V1) | `.AddResponse()`, `.DeletedAddresses` |
| `FakeGeoLookupApiV1_1` | `IGeoLookupApi` (V1.1) | `.AddCityResponse()`, `.AddInsightsResponse()` |
| `FakeApiInfoApi` | `IApiInfoApi` | `.WithInfo()` |
| `FakeApiHealthApi` | `IApiHealthApi` | `.WithStatusCode()` |
